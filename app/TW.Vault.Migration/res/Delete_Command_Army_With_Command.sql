BEGIN
	DELETE FROM tw.command_army
			WHERE army_id = OLD.army_id AND world_id = OLD.world_id;
RETURN NEW;
END;
