-- Create pet_star_up function
-- This function upgrades pet star by 1 using items
CREATE OR REPLACE FUNCTION public.pet_star_up(
    p_pet_id UUID,
    p_item_ids TEXT[],
    p_item_qtys INT[]
)
RETURNS VOID
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    v_user_id UUID;
    v_current_star INT;
    i INT;
    v_inventory_qty INT;
BEGIN
    -- 1. Xác thực người dùng
    v_user_id := auth.uid();
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'Vui lòng đăng nhập.';
    END IF;

    -- 2. Xác thực Pet
    SELECT star INTO v_current_star FROM user_pets 
    WHERE id = p_pet_id AND user_id = v_user_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Không tìm thấy Pet hoặc Pet không thuộc quyền sở hữu của bạn.';
    END IF;

    -- 3. Kiểm tra xem người chơi CÓ ĐỦ TẤT CẢ nguyên liệu không
    FOR i IN 1..array_length(p_item_ids, 1) LOOP
        IF p_item_qtys[i] > 0 THEN
            SELECT quantity INTO v_inventory_qty 
            FROM user_inventory 
            WHERE user_id = v_user_id AND item_id = p_item_ids[i];

            IF v_inventory_qty IS NULL OR v_inventory_qty < p_item_qtys[i] THEN
                RAISE EXCEPTION 'Không đủ nguyên liệu: % (Cần: %, Đang có: %)', p_item_ids[i], p_item_qtys[i], COALESCE(v_inventory_qty, 0);
            END IF;
        END IF;
    END LOOP;

    -- 4. Trừ nguyên liệu
    FOR i IN 1..array_length(p_item_ids, 1) LOOP
        IF p_item_qtys[i] > 0 THEN
            UPDATE user_inventory
            SET quantity = quantity - p_item_qtys[i]
            WHERE user_id = v_user_id AND item_id = p_item_ids[i];
            
            DELETE FROM user_inventory WHERE user_id = v_user_id AND quantity <= 0;
        END IF;
    END LOOP;

    -- 5. Cộng 1 Sao cho Pet
    UPDATE user_pets
    SET star = v_current_star + 1
    WHERE id = p_pet_id AND user_id = v_user_id;
END;
$$;

-- Grant execute permission to authenticated users
GRANT EXECUTE ON FUNCTION public.pet_star_up(UUID, TEXT[], INT[]) TO authenticated;
