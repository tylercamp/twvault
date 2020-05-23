DECLARE
	army_id_stationed int8;
	army_id_traveling int8;
	army_id_owned int8;
	army_id_losses int8;
	army_id_at_home int8;
	army_id_supporting int8;

	our_tribe int8;
	attacker_tribe int8;
	defender_tribe int8;
BEGIN
	/*********************************************
	 * SETUP
	 */

	/* Get tribe IDs */
	SELECT tribe_id FROM tw_provided.player
	INTO our_tribe
	WHERE player_id = (
		SELECT player_id FROM security.user
		WHERE uid = (
			SELECT uid FROM security.transaction
			WHERE tx_id = NEW.tx_id
		) AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
	) AND world_id = NEW.world_id;

	SELECT tribe_id FROM tw_provided.player
	INTO attacker_tribe
	WHERE player_id = NEW.attacker_player_id AND world_id = NEW.world_id;

	SELECT tribe_id FROM tw_provided.player
	INTO defender_tribe
	WHERE player_id = NEW.defender_player_id AND world_id = NEW.world_id;
	
	/* If the attacker village does not exist in current_village, add it. */
	IF (
		SELECT NOT EXISTS (
			SELECT village_id FROM tw.current_village
			WHERE village_id = NEW.attacker_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
		)
	)
	THEN
		INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_stationed;
		INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_traveling;
		INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_owned;
		INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_losses;
		INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_at_home;
		INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_supporting;

		INSERT INTO tw.current_village (village_id, world_id, army_stationed_id, army_traveling_id, army_owned_id, army_recent_losses_id, army_at_home_id, army_supporting_id, access_group_id)
			VALUES
				(
					NEW.attacker_village_id,
					NEW.world_id,
					army_id_stationed,
					army_id_traveling,
					army_id_owned,
					army_id_losses,
					army_id_at_home,
					army_id_supporting,
					NEW.access_group_id
				)
		ON CONFLICT ON CONSTRAINT villages_pk
		DO NOTHING;
	END IF;
	
	/* If the defender village does not exist in current_village, add it. */
	IF (
		SELECT NOT EXISTS (
			SELECT village_id FROM tw.current_village
			WHERE village_id = NEW.defender_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
		)
	)
	THEN
		INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_stationed;
		INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_traveling;
		INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_owned;
		INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_losses;
		INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_at_home;
		INSERT INTO tw.current_army (world_id) VALUES(NEW.world_id) RETURNING army_id INTO army_id_supporting;

		INSERT INTO tw.current_village (village_id, world_id, army_stationed_id, army_traveling_id, army_owned_id, army_recent_losses_id, army_at_home_id, army_supporting_id, access_group_id)
			VALUES
				(
					NEW.defender_village_id,
					NEW.world_id,
					army_id_stationed,
					army_id_traveling,
					army_id_owned,
					army_id_losses,
					army_id_at_home,
					army_id_supporting,
					NEW.access_group_id
				)
		ON CONFLICT ON CONSTRAINT villages_pk
		DO NOTHING;
	END IF;
	
	
	
	/*********************************************
	 * DEFENDER CALCULATIONS
	 */
	
	/* If the defender is not in our tribe, update the army values for them. */
	IF (COALESCE(defender_tribe, -1) <> our_tribe) THEN

		/* Update defender traveling army */
		IF NEW.defender_traveling_army_id IS NOT null
		THEN
			IF ( /* If this is more recent data */
				SELECT last_updated FROM tw.current_army
				WHERE army_id = (
					SELECT army_traveling_id FROM tw.current_village
					WHERE village_id = NEW.defender_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
					LIMIT 1
				)
				LIMIT 1
			) < NEW.occured_at
			OR ( /* Or if there is no data */
				SELECT last_updated FROM tw.current_army
				WHERE army_id = (
					SELECT army_traveling_id FROM tw.current_village
					WHERE village_id = NEW.defender_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
					LIMIT 1
				)
				LIMIT 1
			) IS NULL
			THEN
				/* Update current_village owned and traveling armies to equal this amount shown. */
				UPDATE tw.current_army AS a
				SET
					last_updated = NEW.occured_at,
					spear=COALESCE(b.spear, a.spear), sword=COALESCE(b.sword, a.sword), axe=COALESCE(b.axe, a.axe), archer=COALESCE(b.archer, a.archer), spy=COALESCE(b.spy, a.spy),
					light=COALESCE(b.light, a.light), marcher=COALESCE(b.marcher, a.marcher), heavy=COALESCE(b.heavy, a.heavy), ram=COALESCE(b.ram, a.ram), catapult=COALESCE(b.catapult, a.catapult),
					knight=COALESCE(b.knight, a.knight), snob=COALESCE(b.snob, a.snob), militia=COALESCE(b.militia, a.militia)
				FROM (
					/* Grab values from the report */
					SELECT * FROM tw.report_army
					WHERE tw.report_army.army_id = NEW.defender_traveling_army_id AND world_id = NEW.world_id
				) AS b
				WHERE a.army_id = (
					/* Grab the army we're updating */
					SELECT army_traveling_id FROM tw.current_village
					WHERE village_id = NEW.defender_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
					LIMIT 1
				);
			END IF;
		END IF;

		/* Update defender losses army */
		IF NEW.defender_army_id IS NOT null AND NEW.defender_losses_army_id IS NOT null THEN
			IF (
				SELECT last_updated FROM tw.current_army
				WHERE army_id = (
					SELECT army_stationed_id FROM tw.current_village
					WHERE village_id = NEW.defender_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
					LIMIT 1
				)
				LIMIT 1
			) < NEW.occured_at
			OR (
				SELECT last_updated FROM tw.current_army
				WHERE army_id = (
					SELECT army_stationed_id FROM tw.current_village
					WHERE village_id = NEW.defender_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
					LIMIT 1
				) LIMIT 1
			) IS NULL
			THEN
				/* Update current_village stationed with defenderArmy - defenderLosses */
				UPDATE tw.current_army AS a
				SET
					last_updated = NEW.occured_at,
					spear    = greatest(b.spear    - c.spear,    0), sword  = greatest(b.sword  - c.sword,  0), axe   = greatest(b.axe   - c.axe,   0),
					archer   = greatest(b.archer   - c.archer,   0), spy    = greatest(b.spy    - c.spy,    0), light = greatest(b.light - c.light, 0),
					marcher  = greatest(b.marcher  - c.marcher,  0), heavy  = greatest(b.heavy  - c.heavy,  0), ram   = greatest(b.ram   - c.ram,   0),
					catapult = greatest(b.catapult - c.catapult, 0), knight = greatest(b.knight - c.knight, 0), snob  = greatest(b.snob  - c.snob,  0),
					militia  = greatest(b.militia  - c.militia,  0)
				FROM
					(
						SELECT * FROM tw.report_army
						WHERE tw.report_army.army_id = NEW.defender_army_id AND tw.report_army.world_id = NEW.world_id
					) AS b, 
					(
						SELECT * FROM tw.report_army
						WHERE tw.report_army.army_id = NEW.defender_losses_army_id AND tw.report_army.world_id = NEW.world_id
					) AS c
				WHERE a.army_id = (
					SELECT army_stationed_id FROM tw.current_village
					WHERE village_id = NEW.defender_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
					LIMIT 1
				);
			END IF;
		END IF;
	END IF;



	/*********************************************
	 * ATTACKER CALCULATIONS
	 */
	
	/* If the attacker is not in our tribe, update the army values for them. */
 	IF COALESCE(attacker_tribe, -1) <> our_tribe THEN
	
		/* Update current_army if we have more recent data or there is no current_army data */
		IF (
			SELECT last_updated FROM tw.current_army
			WHERE army_id = (
				SELECT army_owned_id FROM tw.current_village
				WHERE village_id = NEW.attacker_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
				LIMIT 1
			) LIMIT 1
		) < NEW.occured_at
		OR (
			SELECT last_updated FROM tw.current_army
			WHERE army_id = (
				SELECT army_owned_id FROM tw.current_village
				WHERE village_id = NEW.attacker_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
				LIMIT 1
			) LIMIT 1
		) IS NULL
		THEN
		
			/* Update current_village owned_army with values (attacker_army_id - attacker_losses_army_id) if the attacker_army_id values >= owned_army */
			UPDATE tw.current_army AS a
			SET
				/*
				 * a: army being updated
				 * b: attacking army
				 * c: attacker losses
				 * d: army belonging to the attacker village
				 */
				last_updated = NEW.occured_at,
				spear    = greatest( COALESCE(d.spear, 0) - c.spear,       (b.spear - c.spear),       0 ),
				sword    = greatest( COALESCE(d.sword, 0) - c.sword,       (b.sword - c.sword),       0 ),
				axe      = greatest( COALESCE(d.axe, 0) - c.axe,           (b.axe - c.axe),           0 ),
				archer   = greatest( COALESCE(d.archer, 0) - c.archer,     (b.archer - c.archer),     0 ),
				spy      = greatest( COALESCE(d.spy, 0) - c.spy,           (b.spy - c.spy),           0 ),
				light    = greatest( COALESCE(d.light, 0) - c.light,       (b.light - c.light),       0 ),
				marcher  = greatest( COALESCE(d.marcher, 0) - c.marcher,   (b.marcher - c.marcher),   0 ),
				heavy    = greatest( COALESCE(d.heavy, 0) - c.heavy,       (b.heavy - c.heavy),       0 ),
				ram      = greatest( COALESCE(d.ram, 0) - c.ram,           (b.ram - c.ram),           0 ),
				catapult = greatest( COALESCE(d.catapult, 0) - c.catapult, (b.catapult - c.catapult), 0 ),
				knight   = greatest( COALESCE(d.knight, 0) - c.knight,     (b.knight - c.knight),     0 ),
				snob     = greatest( COALESCE(d.snob, 0) - c.snob,         (b.snob - c.snob),         0 ),
				militia  = greatest( COALESCE(d.militia, 0) - c.militia,   (b.militia - c.militia),   0 )
			FROM
				(
					SELECT * FROM tw.report_army
					WHERE tw.report_army.army_id = NEW.attacker_army_id AND tw.report_army.world_id = NEW.world_id
				) AS b, 
				(
					SELECT * FROM tw.report_army
					WHERE tw.report_army.army_id = NEW.attacker_losses_army_id AND tw.report_army.world_id = NEW.world_id
				) AS c,
				(
					SELECT * FROM tw.current_army
					WHERE army_id = (
						SELECT army_owned_id FROM tw.current_village
						WHERE village_id = NEW.attacker_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
						LIMIT 1
					)
				) AS d
			WHERE a.army_id = (
				SELECT army_owned_id FROM tw.current_village
				WHERE village_id = NEW.attacker_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
				LIMIT 1
			);
			
		END IF;
		
	END IF;
	
	
	
	/*********************************************
	 * BUILDING CALCULATIONS
	 */
	
	/* Update building if the defender is not in our tribe and building data is available from the report */
	IF (COALESCE(defender_tribe, -1) <> our_tribe)
	AND (
		SELECT EXISTS (
			SELECT report_building_id FROM tw.report_building
			WHERE report_building_id = NEW.building_id AND world_id = NEW.world_id
		)
	)
	THEN
		/* Create row for current_building if necessary */
		IF (
			SELECT NOT EXISTS (
				SELECT village_id FROM tw.current_building
				WHERE village_id = NEW.defender_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
			)
		)
		THEN
			INSERT INTO tw.current_building (village_id, world_id, access_group_id) VALUES (NEW.defender_village_id, NEW.world_id, NEW.access_group_id)
			ON CONFLICT ON CONSTRAINT current_building_pkey
			DO NOTHING;
		END IF;
		
		/* Update building data if this is more recent or there is no existing current_building data */
		IF ((SELECT last_updated FROM tw.current_building WHERE village_id = NEW.defender_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id) IS NULL)
		OR ((SELECT last_updated FROM tw.current_building WHERE village_id = NEW.defender_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id) < NEW.occured_at)
		THEN				
			UPDATE tw.current_building AS a
			SET
				last_updated = NEW.occured_at,
				main   = COALESCE(b.main, a.main),     stable       = COALESCE(b.stable, a.stable),             garage = COALESCE(b.garage, a.garage),
				church = COALESCE(b.church, a.church), first_church = COALESCE(b.first_church, a.first_church), smith = COALESCE(b.smith, a.smith),
				place  = COALESCE(b.place, a.place),   statue       = COALESCE(b.statue, a.statue),             market = COALESCE(b.market, a.market),
				wood   = COALESCE(b.wood, a.wood),     stone        = COALESCE(b.stone, a.stone),               iron = COALESCE(b.iron, a.iron),
				farm   = COALESCE(b.farm, a.farm),     storage      = COALESCE(b.storage, a.storage),           hide = COALESCE(b.hide, a.hide),
				wall   = COALESCE(b.wall, a.wall),     watchtower   = COALESCE(b.watchtower, a.watchtower),     barracks = COALESCE(b.barracks, a.barracks),
				snob   = COALESCE(b.snob, a.snob)
			FROM (
				SELECT * FROM tw.report_building WHERE report_building_id = NEW.building_id AND world_id = NEW.world_id
			) as b
			WHERE a.village_id = NEW.defender_village_id AND a.world_id = NEW.world_id AND a.access_group_id = NEW.access_group_id;
		END IF;
	END IF;
	
	
	
	/*********************************************
	 * LOYALTY CALCULATIONS
	 */
	 
	/* If the report has loyalty data */
	IF (NEW.loyalty IS NOT NULL) THEN
		/* If we have more recent loyalty data or no loyalty has been previously recorded */
		IF (
			SELECT loyalty_last_updated FROM tw.current_village
			WHERE village_id = NEW.defender_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
		) IS NULL
		OR (
			SELECT loyalty_last_updated FROM tw.current_village
			WHERE village_id = NEW.defender_village_id AND world_id = NEW.world_id AND access_group_id = NEW.access_group_id
		) < NEW.occured_at
		THEN
			CASE
				WHEN (NEW.loyalty < 0) THEN
					UPDATE tw.current_village AS a
					SET loyalty = 25, loyalty_last_updated = NEW.occured_at
					WHERE a.village_id = NEW.defender_village_id AND a.world_id = NEW.world_id AND access_group_id = NEW.access_group_id;
				ELSE
					UPDATE tw.current_village AS a
					SET loyalty = NEW.loyalty, loyalty_last_updated = NEW.occured_at
					WHERE a.village_id = NEW.defender_village_id AND a.world_id = NEW.world_id AND access_group_id = NEW.access_group_id;
			END CASE;
		END IF;
	END IF;
	
	RETURN NEW;
END;