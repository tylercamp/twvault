DECLARE 
	access_group RECORD;
	army_id_stationed int8;
	army_id_traveling int8;
	army_id_owned int8;
	army_id_losses int8;
	army_id_at_home int8;
	army_id_supporting int8;
	agid int8;
BEGIN
	FOR access_group IN SELECT id
		FROM security.access_group
		WHERE world_id = NEW.world_id
	LOOP
		agid := access_group.id;
		army_id_stationed := army_stationed_id FROM tw.current_village WHERE village_id = NEW.village_id AND world_id = NEW.world_id AND access_group_id = agid;
		army_id_traveling := army_traveling_id FROM tw.current_village WHERE village_id = NEW.village_id AND world_id = NEW.world_id AND access_group_id = agid;
		army_id_owned := army_owned_id FROM tw.current_village WHERE village_id = NEW.village_id AND world_id = NEW.world_id AND access_group_id = agid;
		army_id_at_home := army_at_home_id FROM tw.current_village WHERE village_id = NEW.village_id AND world_id = NEW.world_id AND access_group_id = agid;
		army_id_supporting := army_supporting_id FROM tw.current_village WHERE village_id = NEW.village_id AND world_id = NEW.world_id AND access_group_id = agid;

		/* If the village is not already recognized in current_village (we have no data on it) then add it, and add rows in current_army for it. */
		IF (SELECT NOT EXISTS(SELECT village_id FROM tw.current_village WHERE village_id = NEW.village_id AND world_id = NEW.world_id AND access_group_id = agid)) THEN							
			INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_stationed;
			INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_traveling;
			INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_owned;
			INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_losses;
			INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_at_home;
			INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_supporting;

			INSERT INTO tw.current_village (village_id, world_id, access_group_id, army_stationed_id, army_traveling_id, army_owned_id, army_recent_losses_id, army_at_home_id, army_supporting_id)
				VALUES
					(
						NEW.village_id,
						NEW.world_id,
						agid,
						army_id_stationed,
						army_id_traveling,
						army_id_owned,
						army_id_losses,
						army_id_at_home,
						army_id_supporting
					)
			ON CONFLICT ON CONSTRAINT villages_pk
			DO NOTHING;
		END IF;

		/* Set all current_army's for this village to NULL (if it already existed, otherwise these values are already NULL) */
		UPDATE tw.current_army
			SET spear=NULL, sword=NULL, axe=NULL, archer=NULL, spy=NULL, light=NULL, marcher=NULL, heavy=NULL, ram=NULL, catapult=NULL, knight=NULL, snob=NULL, militia=NULL
			WHERE tw.current_army.army_id IN (army_id_stationed, army_id_traveling, army_id_owned, army_id_losses, army_id_at_home, army_id_supporting) AND tw.current_army.world_id = NEW.world_id;
		DELETE FROM tw.command
			WHERE source_village_id = NEW.village_id AND world_id = NEW.world_id AND access_group_id = agid;

		/* Set this village's loyalty to 25, and the timestamp of when it was conquered (for calculating current loyalty) */
		UPDATE tw.current_village
			SET loyalty = 25, loyalty_last_updated = to_timestamp(NEW.unix_timestamp)
			WHERE tw.current_village.village_id = NEW.village_id AND tw.current_village.world_id = NEW.world_id AND access_group_id = agid;
	END LOOP;
RETURN NEW;
END;