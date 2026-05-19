-- Add reward_items column to battle_logs table
-- This column stores the items dropped/awarded from battle
ALTER TABLE battle_logs 
ADD COLUMN IF NOT EXISTS reward_items JSONB DEFAULT '[]'::jsonb;

-- Add comment for documentation
COMMENT ON COLUMN battle_logs.reward_items IS 'Array of items rewarded from battle (item_id, quantity, etc.)';
