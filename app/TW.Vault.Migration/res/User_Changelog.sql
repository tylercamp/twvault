BEGIN
	IF (TG_OP = 'DELETE') THEN
		INSERT INTO security.user_log (uid, player_id, permissions_level, "label", enabled, auth_token, world_id, key_source, transaction_time, admin_auth_token, admin_player_id, is_read_only, tx_id, access_group_id, operation_type)
			SELECT OLD.*, 'DELETE';
		RETURN OLD;
    ELSIF (TG_OP = 'UPDATE') THEN
		INSERT INTO security.user_log (uid, player_id, permissions_level, "label", enabled, auth_token, world_id, key_source, transaction_time, admin_auth_token, admin_player_id, is_read_only, tx_id, access_group_id, operation_type)
			SELECT NEW.*, 'UPDATE';
		RETURN NEW;
	ELSEIF (TG_OP = 'INSERT') THEN
		INSERT INTO security.user_log (uid, player_id, permissions_level, "label", enabled, auth_token, world_id, key_source, transaction_time, admin_auth_token, admin_player_id, is_read_only, tx_id, access_group_id, operation_type)
			SELECT NEW.*, 'INSERT';
		RETURN NEW;
	END IF;
END;
