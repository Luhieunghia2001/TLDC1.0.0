-- Drop old function if exists
DROP FUNCTION IF EXISTS public.use_pet_exp_potion_secure(UUID, TEXT);

-- Create/Update use_pet_exp_potion_secure function
-- This function uses EXP potion to increase pet EXP and level up
CREATE OR REPLACE FUNCTION public.use_pet_exp_potion_secure(
    p_pet_id UUID,
    p_item_id TEXT
)
RETURNS VOID
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    v_user_id UUID;
    v_inventory_qty INT;
    v_current_level INT;
    v_current_exp INT;
    v_max_exp INT;
BEGIN
    -- 1. Xác thực người dùng
    v_user_id := auth.uid();
    IF v_user_id IS NULL THEN RAISE EXCEPTION 'Vui lòng đăng nhập.'; END IF;

    -- 2. Kiểm tra tồn kho
    SELECT quantity INTO v_inventory_qty FROM user_inventory WHERE user_id = v_user_id AND item_id = p_item_id;
    IF v_inventory_qty IS NULL OR v_inventory_qty < 1 THEN RAISE EXCEPTION 'Không đủ vật phẩm.'; END IF;

    -- 3. Trừ đồ
    UPDATE user_inventory SET quantity = quantity - 1 WHERE user_id = v_user_id AND item_id = p_item_id;
    DELETE FROM user_inventory WHERE user_id = v_user_id AND quantity <= 0;

    -- 4. Lấy thông tin Pet hiện tại
    SELECT level, current_exp INTO v_current_level, v_current_exp FROM user_pets WHERE id = p_pet_id AND user_id = v_user_id;
    IF NOT FOUND THEN RAISE EXCEPTION 'Không tìm thấy Pet.'; END IF;

    -- 5. Kiểm tra level pet không vượt quá 100
    IF v_current_level >= 100 THEN
        RAISE EXCEPTION 'Pet đã đạt level tối đa (100).';
    END IF;

    -- 6. Lấy level của player
    DECLARE
        v_player_level INT;
    BEGIN
        SELECT level INTO v_player_level FROM players WHERE id = v_user_id;
        IF v_player_level IS NULL THEN
            RAISE EXCEPTION 'Không tìm thấy thông tin player.';
        END IF;

        -- Kiểm tra pet level phải thấp hơn player level
        IF v_current_level >= v_player_level THEN
            RAISE EXCEPTION 'Level Pet phải thấp hơn Level Player (Pet: %, Player: %).', v_current_level, v_player_level;
        END IF;
    END;

    -- 6. Lấy EXP amount từ item_templates
    DECLARE
        v_exp_to_add INT;
        v_item_type TEXT;
    BEGIN
        SELECT effect_value, item_type INTO v_exp_to_add, v_item_type 
        FROM item_templates WHERE id = p_item_id;

        IF v_item_type IS DISTINCT FROM 'ExpPotion' OR v_exp_to_add IS NULL THEN
            RAISE EXCEPTION 'Vật phẩm này không phải là Bình EXP hoặc không tồn tại trên hệ thống!';
        END IF;

        v_current_level := COALESCE(v_current_level, 1);
        v_current_exp := COALESCE(v_current_exp, 0) + v_exp_to_add;
        
        -- 7. Xử lý level up với loop guard
        FOR i IN 1..1000 LOOP
            v_max_exp := v_current_level * 100;
            IF v_max_exp <= 0 THEN v_max_exp := 100; END IF;
            EXIT WHEN v_current_exp < v_max_exp;
            
            -- Kiểm tra nếu đã đạt max level
            IF v_current_level >= 100 THEN
                v_current_exp := v_max_exp - 1; -- Set exp vừa dưới max để không level up nữa
                EXIT;
            END IF;
            
            v_current_exp := v_current_exp - v_max_exp;
            v_current_level := v_current_level + 1;
        END LOOP;
    END;

    -- 8. Cập nhật level và exp của Pet
    UPDATE user_pets SET level = v_current_level, current_exp = v_current_exp WHERE id = p_pet_id AND user_id = v_user_id;
END;
$$;

-- Grant execute permission to authenticated users
GRANT EXECUTE ON FUNCTION public.use_pet_exp_potion_secure(UUID, TEXT) TO authenticated;
