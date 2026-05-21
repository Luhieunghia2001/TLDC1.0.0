-- Add equipment columns to user_pets table
ALTER TABLE user_pets 
ADD COLUMN IF NOT EXISTS helmet_id TEXT DEFAULT NULL,
ADD COLUMN IF NOT EXISTS armor_id TEXT DEFAULT NULL,
ADD COLUMN IF NOT EXISTS weapon_id TEXT DEFAULT NULL,
ADD COLUMN IF NOT EXISTS boots_id TEXT DEFAULT NULL,
ADD COLUMN IF NOT EXISTS wings_id TEXT DEFAULT NULL,
ADD COLUMN IF NOT EXISTS amulet_id TEXT DEFAULT NULL;

-- Create equip_pet_item function
CREATE OR REPLACE FUNCTION public.equip_pet_item(
    p_pet_id UUID,
    p_slot TEXT,
    p_item_id TEXT
)
RETURNS VOID
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    v_user_id UUID;
    v_inventory_qty INT;
    v_old_item_id TEXT;
    v_pet RECORD;
BEGIN
    -- 1. Xác thực người dùng
    v_user_id := auth.uid();
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'Vui lòng đăng nhập.';
    END IF;

    -- 2. Xác thực Pet và lấy trang bị hiện tại
    SELECT * INTO v_pet FROM user_pets 
    WHERE id = p_pet_id AND user_id = v_user_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Không tìm thấy Pet hoặc Pet không thuộc sở hữu của bạn.';
    END IF;

    -- 3. Kiểm tra xem trang bị đang ở trong rương hay đang mặc trên pet khác của người chơi
    SELECT quantity INTO v_inventory_qty 
    FROM user_inventory 
    WHERE user_id = v_user_id AND item_id = p_item_id;

    IF v_inventory_qty IS NOT NULL AND v_inventory_qty >= 1 THEN
        -- Trường hợp A: Có trong rương -> Trừ 1 trang bị từ rương
        UPDATE user_inventory 
        SET quantity = quantity - 1 
        WHERE user_id = v_user_id AND item_id = p_item_id;
        
        DELETE FROM user_inventory WHERE user_id = v_user_id AND quantity <= 0;
    ELSE
        -- Trường hợp B: Không có trong rương -> Kiểm tra xem có đang mặc trên pet khác không để gỡ
        DECLARE
            v_other_pet_id UUID;
        BEGIN
            SELECT id INTO v_other_pet_id FROM user_pets
            WHERE user_id = v_user_id 
              AND (helmet_id = p_item_id 
                   OR armor_id = p_item_id 
                   OR weapon_id = p_item_id 
                   OR boots_id = p_item_id 
                   OR wings_id = p_item_id 
                   OR amulet_id = p_item_id)
            LIMIT 1;

            IF v_other_pet_id IS NOT NULL THEN
                -- Gỡ trang bị từ pet khác bằng cách set NULL cho các slot tương ứng có ID này
                UPDATE user_pets 
                SET 
                    helmet_id = CASE WHEN helmet_id = p_item_id THEN NULL ELSE helmet_id END,
                    armor_id = CASE WHEN armor_id = p_item_id THEN NULL ELSE armor_id END,
                    weapon_id = CASE WHEN weapon_id = p_item_id THEN NULL ELSE weapon_id END,
                    boots_id = CASE WHEN boots_id = p_item_id THEN NULL ELSE boots_id END,
                    wings_id = CASE WHEN wings_id = p_item_id THEN NULL ELSE wings_id END,
                    amulet_id = CASE WHEN amulet_id = p_item_id THEN NULL ELSE amulet_id END
                WHERE id = v_other_pet_id AND user_id = v_user_id;
            ELSE
                RAISE EXCEPTION 'Trang bị không tồn tại trong rương và cũng không được mặc bởi Pet nào của bạn.';
            END IF;
        END;
    END IF;

    -- 4. Xác định trang bị cũ trong slot và cập nhật trang bị mới
    IF p_slot = 'helmet' THEN
        v_old_item_id := v_pet.helmet_id;
        UPDATE user_pets SET helmet_id = p_item_id WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'armor' THEN
        v_old_item_id := v_pet.armor_id;
        UPDATE user_pets SET armor_id = p_item_id WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'weapon' THEN
        v_old_item_id := v_pet.weapon_id;
        UPDATE user_pets SET weapon_id = p_item_id WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'boots' THEN
        v_old_item_id := v_pet.boots_id;
        UPDATE user_pets SET boots_id = p_item_id WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'wings' THEN
        v_old_item_id := v_pet.wings_id;
        UPDATE user_pets SET wings_id = p_item_id WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'amulet' THEN
        v_old_item_id := v_pet.amulet_id;
        UPDATE user_pets SET amulet_id = p_item_id WHERE id = p_pet_id AND user_id = v_user_id;
    ELSE
        RAISE EXCEPTION 'Vị trí trang bị không hợp lệ: %', p_slot;
    END IF;

    -- 5. Nếu có trang bị cũ, trả lại rương đồ cho người chơi
    IF v_old_item_id IS NOT NULL AND v_old_item_id <> '' THEN
        INSERT INTO user_inventory (user_id, item_id, quantity)
        VALUES (v_user_id, v_old_item_id, 1)
        ON CONFLICT (user_id, item_id) 
        DO UPDATE SET quantity = user_inventory.quantity + 1;
    END IF;
END;
$$;

-- Create unequip_pet_item function
CREATE OR REPLACE FUNCTION public.unequip_pet_item(
    p_pet_id UUID,
    p_slot TEXT
)
RETURNS VOID
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    v_user_id UUID;
    v_equipped_item_id TEXT;
    v_pet RECORD;
BEGIN
    -- 1. Xác thực người dùng
    v_user_id := auth.uid();
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'Vui lòng đăng nhập.';
    END IF;

    -- 2. Xác thực Pet
    SELECT * INTO v_pet FROM user_pets 
    WHERE id = p_pet_id AND user_id = v_user_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Không tìm thấy Pet hoặc Pet không thuộc sở hữu của bạn.';
    END IF;

    -- 3. Xác định trang bị hiện tại trong slot và gỡ bỏ
    IF p_slot = 'helmet' THEN
        v_equipped_item_id := v_pet.helmet_id;
        UPDATE user_pets SET helmet_id = NULL WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'armor' THEN
        v_equipped_item_id := v_pet.armor_id;
        UPDATE user_pets SET armor_id = NULL WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'weapon' THEN
        v_equipped_item_id := v_pet.weapon_id;
        UPDATE user_pets SET weapon_id = NULL WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'boots' THEN
        v_equipped_item_id := v_pet.boots_id;
        UPDATE user_pets SET boots_id = NULL WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'wings' THEN
        v_equipped_item_id := v_pet.wings_id;
        UPDATE user_pets SET wings_id = NULL WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'amulet' THEN
        v_equipped_item_id := v_pet.amulet_id;
        UPDATE user_pets SET amulet_id = NULL WHERE id = p_pet_id AND user_id = v_user_id;
    ELSE
        RAISE EXCEPTION 'Vị trí trang bị không hợp lệ: %', p_slot;
    END IF;

    -- 4. Nếu có trang bị, trả lại rương đồ
    IF v_equipped_item_id IS NOT NULL AND v_equipped_item_id <> '' THEN
        INSERT INTO user_inventory (user_id, item_id, quantity)
        VALUES (v_user_id, v_equipped_item_id, 1)
        ON CONFLICT (user_id, item_id) 
        DO UPDATE SET quantity = user_inventory.quantity + 1;
    END IF;
END;
$$;

-- Grant execute permissions to authenticated users
GRANT EXECUTE ON FUNCTION public.equip_pet_item(UUID, TEXT, TEXT) TO authenticated;
GRANT EXECUTE ON FUNCTION public.unequip_pet_item(UUID, TEXT) TO authenticated;
