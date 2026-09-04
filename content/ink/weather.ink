# Weather-specific narrative scenes

=== event_khamsin ===
The sky turns the color of old brass. By noon the wind has teeth. Fine sand finds every gap in the window frame, every crack in the door, every soft membrane it can reach. Cairo slows under the khamsin. People move with their eyes narrowed and their faces wrapped.

Your mother pulls the thin curtain across her face and breathes through cloth. The rooftop solar batteries drop to reserve. Delivery drones abandon their routes and land wherever they can. Outside, a fruit cart overturns and nobody bothers to pick it up.

*   [Stay inside and seal the gaps]
    You stuff newspaper into the window frames and dampen a sheet with yesterday's wash water. The apartment becomes a cave, dark and close, but the sand stays mostly outside. Your mother coughs less. By afternoon, the two of you sit in the dim quiet listening to the wind tire itself out.
    # STRESS:1
    # ENERGY:-5
    # MESSAGE:The khamsin howls outside. You make the flat as tight as you can and wait.
    -> DONE

*   [Venture out to secure supplies before it gets worse]
    The sand hits like a slap. You wrap your headscarf tight and move low along the wall. The forn is already closed, the ahwa chairs stacked, the street emptied of everyone except the truly desperate and the truly stubborn. You find a shop still open and overpay for water and bread, but you return with something to show for the burning in your lungs.
    # STRESS:5
    # ENERGY:-8
    # HEALTH:-3
    # FOOD:3
    # MESSAGE:You brave the khamsin for supplies. The price was paid in lungs and skin.
    -> DONE

=== event_khamsin_prisoner ===
The khamsin does not bother you the way it bothers others. Prison taught you how to sit in a room that felt like an oven and wait for time to pass without going mad. The sand in the air reminds you of the dust that settled on the cell floor every morning, and the memory is not gentle, but it is familiar, and familiarity is a kind of survival.

Your mother watches you staring at the sealed window and asks what you are thinking. You say nothing. She does not push.
# STRESS:5
# MESSAGE:The khamsin carries you back to two years of dust and waiting.
-> DONE

=== event_khamsin_sudanese ===
The other Sudanese families gather in the largest apartment on the third floor. Someone has sealed the windows with plastic sheeting. Someone else has made tea despite the gas flame struggling against the wind pressure. The room smells of close bodies, cedar incense, and the particular patience of people who have already survived worse than weather.

Your mother sits with the older women and speaks Arabic with a Khartoum accent that the others mirror without thinking. For a few hours, the khamsin howls outside and the room inside becomes a different country.
# STRESS:-3
# NPC_TRUST:PharmacistMariam,2
# MESSAGE:The Sudanese community shelters together through the khamsin. The storm outside makes the room inside feel like home.
-> DONE

=== event_rain_leak ===
Rain has become too scarce for the city to practice handling it. It starts as a murmur on the roof and builds to a roar. By the second hour, water finds the crack above the kitchen door. A drip becomes a stream, then a line tracing the wall toward the floor.

Your mother moves her mattress away from the wall and says nothing, which is how she communicates the loudest disappointment.

*   [Try to patch it with what you have]
    You climb on a chair with a plastic bag, some tape, and the kind of optimism that Cairo punishes. The patch holds for an hour. Then it doesn't. The water resumes its route, and you resume yours: mopping, watching, waiting for the rain to stop being angry.
    # STRESS:4
    # ENERGY:-5
    # MESSAGE:You patch the leak as well as you can. It is not well enough.
    -> DONE

*   [Accept it and focus on keeping your mother dry]
    You move everything important to the center of the room and create a small island of dryness around your mother's mattress. The leak becomes background noise, like the microbuses and the neighbors' arguments. Cairo teaches you to live inside damage.
    # STRESS:3
    # MESSAGE:You triage the rain. Your mother stays dry. The wall does not.
    -> DONE

=== event_rain_leak_with_curtain ===
The rain hammers the roof, but the curtain upgrade does its job. No water reaches the mattress, and the window stays tight. The sound of rain on concrete is almost peaceful when it is not accompanied by the sound of water finding its way into your life.

Your mother listens to the rain and says it sounds like Khartoum in August, before the heat broke. For a moment, the flat is not in Cairo. It is anywhere it needs to be.
# STRESS:-3
# MESSAGE:The rain falls, but your home holds. The curtain earns its cost.
-> DONE

=== event_rain_outside ===
You are three blocks from home when the sky opens. The rain is rare, but the old drains are still narrow and packed with dust. The streets of Imbaba turn into rivers of grey water and trash. A microbus hydroplanes past, close enough to splash your gallabiya to the knee.

*   [Find shelter and wait it out]
    You duck into a shop doorway with five other people. The rain is warm. It should be refreshing, but it only makes everything damp and heavy.
    # STRESS:3
    # ENERGY:-3
    # MESSAGE:You wait out the rain in a shop doorway with strangers who do not make eye contact.
    -> DONE

*   [Run for home through the flood]
    You sprint through water that rises above your ankles, slipping on mud and tile fragments. By the time you reach the building, you are soaked through, your shoes are ruined, and your mother meets you at the door with a towel and a look that says she predicted this exact outcome.
    # STRESS:5
    # ENERGY:-6
    # HEALTH:-2
    # MESSAGE:You run home through the flood. The city does not care about your shoes.
    -> DONE

=== event_rain_prisoner ===
The rain on the roof sounds exactly like the rain on the cell block. The same rhythm, the same patience, the same reminder that water will find every gap and wear through every wall given enough time. Your hands grip the edge of the table and will not loosen.

Your mother puts her hand over yours. She does not say anything. She does not have to. Two years of rain and waiting live in the silence between your fingers and the wood.
# STRESS:5
# MESSAGE:The rain carries you back to the cell. Your mother's hand brings you back.
-> DONE

=== event_heatwave ===
The heatwave began before the official alert and outlasted the forecast. Last night never cooled. By ten in the morning, the walls are already giving yesterday's heat back to the room. The fan pushes hot air from one corner to another without cooling anything.

Your mother lies still on her mattress with a wet cloth on her forehead. The salvaged cooling unit waits for the building co-op to release another hour of battery power. Outside, electric taxis have been pulled from the road and the street has gone quiet. Shade cloth stretches between balconies, patched and repatched by the people who live below it.

*   [Stay inside and manage the heat as best you can]
    You keep the curtains drawn, the windows sealed, and the wet cloths coming. The apartment becomes a dim, still cave where the only movement is the slow rotation of the fan and the slower rise and fall of your mother's chest. You watch her and count the hours until evening.
    # STRESS:5
    # ENERGY:-10
    # MESSAGE:The heatwave turns the flat into an oven. You survive by going still.
    -> DONE

*   [Go out to find ice or cold water]
    You walk with your head down, following the narrow strip of shade along the buildings. The ice shop has a line that moves at the speed of melting. By the time you return with a bag already half water, you understand why work has shifted toward dawn and late evening whenever employers allow it.
    # STRESS:7
    # ENERGY:-12
    # HEALTH:-5
    # FOOD:1
    # MESSAGE:You venture into the heatwave for ice. The city takes its cut in sweat and dizziness.
    -> DONE

=== event_heatwave_medical ===
Three years of anatomy lectures taught you what the body does under thermal stress: the blood vessels dilate, sweat stops keeping pace, and electrolytes drain. Knowing the warning signs does not make the heat harmless. It tells you when waiting becomes dangerous.

You position your mother's mattress near the window, keep the air moving, mix salt and sugar into the water, and watch for the specific glassiness that means heat exhaustion is becoming heat stroke. Your hands know what to do even when your medical career did not survive to use the knowledge officially.
# STRESS:-2
# ENERGY:-5
# MESSAGE:Your medical training reduces some of the danger. The heatwave is still a crisis.
-> DONE

=== event_cool_day ===
The rare cool morning arrives with a breeze that does not feel like a hair dryer. The street is still loud and the rent is still due, but people linger outside because nobody trusts the relief to last.

Your mother sits by the window and breathes without wheezing. The tea cools before you finish it. These are the days Cairo gives you so that the other days remain survivable.
# STRESS:-5
# ENERGY:5
# MESSAGE:A cool day in Cairo. The city lets you breathe.
-> DONE

=== event_windy_day ===
The wind tears through Imbaba with the energy of a rumour. It flips awnings, sends plastic bags into elaborate flight patterns, and turns the market into a place where nobody's goods stay where they put them. The koshk owner curses as his chip display becomes a snack tornado.

*   [Help secure the market stalls]
    You spend an hour holding down tarps, weighting tables, and chasing a rolling watermelon three blocks. The vendors thank you with discounts and the kind of respect that comes from shared labor rather than money.
    # ENERGY:-5
    # FOOD:2
    # NPC_TRUST:FixerUmmKarim,2
    # STRESS:-2
    # MESSAGE:You help the market weather the wind. The vendors remember.
    -> DONE

*   [Stay inside and enjoy the noise]
    The wind makes the building creak in a way that is almost musical. You close the windows, make tea, and listen to the city being temporarily rearranged. Your mother says the wind sounds like a woman arguing with God, which is either theology or weather commentary and possibly both.
    # STRESS:-3
    # MESSAGE:You stay inside while the wind reorganizes the street.
    -> DONE

=== event_winter_chill ===
Warmer years have not abolished January. The cold seeps through concrete walls built to shed heat, not hold warmth. The flat is colder inside than outside because at least outside there is sun.

Your mother wraps herself in every cloth in the apartment and still shivers. The gas heater is empty. The blankets are thin. The city that tried to kill you with heat in summer tries with cold in winter, and the only thing that changes is the direction of the suffering.

*   [Buy a gas canister on credit]
    You go to the shop and promise payment next week with the sincerity of someone who has made this exact promise before. The canister arrives, the heater clicks on, and the flat fills with the smell of burning gas and the closest thing to comfort January allows.
    # MONEY:-15
    # STRESS:-2
    # MOTHER_HEALTH:3
    # MESSAGE:You buy gas on credit. The heater changes everything for one night.
    -> DONE

*   [Borrow blankets from Mona]
    Mona appears at the door with two thick wool blankets that have seen better decades but still hold heat. She says they were her mother's, which is both a gift and a trust, and you accept them with the careful gratitude that Cairo reserves for things that cannot be repaid with money.
    # NPC_TRUST:NeighborMona,3
    # STRESS:-3
    # MOTHER_HEALTH:2
    # MESSAGE:Mona lends blankets that carry her mother's warmth across the years.
    -> DONE
