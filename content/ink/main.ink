# SLUMS - Main Ink Story

VAR gender = "female"
VAR background = ""
VAR money = 0
VAR health = 100
VAR energy = 100
VAR hunger = 100
VAR stress = 0
VAR mother_health = 100
VAR food_stockpile = 0
VAR day = 1
VAR district = "Imbaba"
VAR weather = "Clear"
VAR season = "Autumn"
VAR holiday = ""
VAR is_ramadan = false
VAR is_fasting = false
VAR unpaid_rent_days = 0
VAR rent_debt = 0
VAR rent_grace_days = 0
VAR police_pressure = 0
VAR operational_robot_count = 0
VAR active_news_count = 0
VAR infrastructure_disruption_count = 0
VAR mona_trust = 0
VAR salma_trust = 0
VAR conversation_variant = ""
VAR conversation_context = ""
VAR conversation_npc = ""
VAR conversation_opener = 0
VAR conversation_body = 0
VAR crisis_phase = "NotDiscovered"
VAR crisis_evidence = 0
VAR crisis_resources = 0
VAR crisis_condition = 70
VAR crisis_decision = "None"
VAR crisis_resolution_state = "Unresolved"
VAR pending_ending = ""
VAR handset_data_exposure = 0
VAR microgrid_repair_debt = 0
VAR microgrid_storage_condition = 70
VAR transit_permit_review = false
VAR biometric_appeal_pending = false
VAR last_telemedicine_triage_day = 0
VAR allocation_model_confidence = 58
VAR mother_arc_decision = ""
VAR mona_arc_decision = ""
VAR salma_arc_decision = ""
VAR mahmoud_arc_decision = ""
VAR ummkarim_arc_decision = ""

INCLUDE crime.ink
INCLUDE events.ink
INCLUDE npcs.ink
INCLUDE conversations.ink
INCLUDE endings.ink
INCLUDE weather.ink
INCLUDE seasons.ink
INCLUDE community.ink
INCLUDE debt.ink
INCLUDE crisis.ink
INCLUDE central_arcs.ink

=== intro_medical ===
Cairo, 2060.

Summer lasts longer than the calendars admit. Even in October, the concrete gives back heat after midnight. Water reaches the rooftop tanks on a rotating schedule, and every household knows the sound of the pump starting. When it fails, the whole building starts counting buckets.

Wealthy roads carry autonomous electric taxis along mapped corridors. Cargo drones cross the roofs when the wind allows it, and new towers advertise self-tinting solar glass that never reaches Imbaba. Down here, three buildings share a rooftop solar-and-storage co-op. The residents patched it together from Egyptian inverters, retired bus cells, and whatever the repair market could recover. They argue over cooling hours when the grid rations power.

Imbaba has made its own future. Rooftop gardens sit beside old satellite dishes and mesh relays. Nubian and Sudanese recipes travel through voice notes. Women run savings circles and water rosters. Young technicians turn retired delivery bots and broken imports into tools the neighborhood can afford.

The city sells machine learning as convenience: a call-center script, a route planner, an eligibility check, a medical triage service. Each system is useful, narrow, and wrong in ways that cost poor people more than rich ones. Your mother waits for a nurse with a cracked diagnostic patch reader and a cupboard of ordinary medicine.

Three years of medical school.

Three years of dreaming of a white coat, of a stethoscope around your neck, of rounds at Kasr El Ainy instead of counting bus fare and pharmacy prices.

Then Baba died, and the tuition money evaporated with him.

Now you sit in your single room in Imbaba, listening to your mother cough into a washed-thin handkerchief while an autonomous taxi gives up at a broken lane beacon and an old microbus leans on its horn below. The cough sounds wet. Worse than yesterday.

*   [Check on her]
    You kneel beside her mattress. Her forehead is clammy, her breathing labored.
    
    "Mama, we need to get you to a doctor."
    
    She waves you off weakly. "Doctors cost money, habibi. Money we don't have."
    
    You have some medical training. You could try to help her yourself, or you could spend what little you have on a proper consultation.
    -> intro_medical_check_mother

*   [Look for work instead]
    You can't help her if you can't pay for medicine.
    
    You step into the alley behind the building. The first ahwa chairs are scraping the pavement as a cargo drone searches for a safe landing and a ground courier bot waits for a route signal. Oil hisses at the taameya cart while a neighbour tries to revive a discarded handset with a screwdriver. Somebody upstairs is already shouting about money.
    
    This is your world now. Not the bright corridors of the faculty hospital. Not the life people used to promise you.
    
    -> intro_done

=== intro_medical_check_mother ===
*   [Use your medical knowledge to help her]
    You check her pulse with two fingers and the scraps of training still living in your hands. Her fever is moderate. You raise her pillow, coach her breathing, boil water, and wait for the room to cool a little.
    
    "You should have been a doctor," she says, her voice rasping.
    
    "I know, Mama."
    
    -> intro_medical_after_treatment

*   [Promise to find the money for a real doctor]
    "I'll find the money, Mama. I promise."
    
    Her eyes say she has lived too long on promises already. Still, she nods and lets you keep your pride.
    
    -> intro_done

=== intro_medical_after_treatment ===
*   [Continue]
    The morning is still salvageable. Your mother is stable for now.
    
    You have 80 Egyptian pounds folded in a drawer. Rent is due in ten days.
    
    -> intro_done

=== intro_prisoner ===
Cairo, 2060. The city remembers everything that can be uploaded.

The heat stays after dark, trapped between concrete walls. Water is rationed by district pressure and electricity by demand. The cameras rarely lose either. Some hang over shop doors, some sit in public-service kiosks, and some ride on small drones during a police sweep. The state calls it safety. The people who live under it call it being watched by a handset, a landlord, a platform, or a person with the right connection. A biometric match can be wrong, but the appeal still costs a day's wages.

The cell door opened eight months ago.

Your mother aged ten years in the two you were inside. The neighbours still lower their voices when you pass. The file never really closes. People still call it the amn el-dawla file, the way Cairo keeps old names for buildings long after the signs change. It sits somewhere with your name on it, waiting.

You are twenty-six years old. You have a criminal record, a gap in your employment history, and a mother who needs medicine you can't afford.

*   [Think about what happened]
    It was just a protest. A gathering in Tahrir against predictive policing pilots, rationed power, and the price of being misclassified by a software system that never has to meet you. Bread prices, corruption, humiliation, all the things that pile up in a chest until they come out as shouting.
    
    The details don't matter to the employers who reject your applications.
    -> intro_prisoner_reflection

*   [Focus on the present]
    The past is the past. Your mother is coughing in the next room, and that's what matters now.
    
    You have 30 Egyptian pounds. The pharmacy will give you only a few days of her heart medication for that, and even that with a look.
    
    -> intro_done

=== intro_prisoner_reflection ===
*   [Let the anger rise]
    Your fists clench. Two years of your life gone. Your mother getting smaller while you learned the smell of concrete, sweat, and waiting.
    
    But anger won't put food on the table. Anger won't buy her heart medication.
    
    -> intro_done

*   [Push it down]
    You breathe until the room stops narrowing. What's done is done. You came out. Some didn't.
    
    Focus on today. Today you have 30 pounds and a mother who needs you.
    
    -> intro_done

=== intro_sudanese ===
Cairo, 2060. The long hot season presses against the roofs well into autumn. Delivery drones blink when their batteries hold, and courier bots wait behind locked gates for cooler pavement. At street level, families still carry water by hand when the neighborhood pump misses its hour.

You still dream of the Nile in Khartoum.

Before the jets came back. The first war never really ended; it only waited, patient as hunger, until the ceasefire rotted and the explosions began shaking your apartment building at 3 AM again. Before that night, your mother grabbed you by the wrist and said, "We leave now. Take only what you can carry."

That was three years ago.

Cairo was supposed to be temporary. A station between one life and the next. But wars have a way of turning the temporary into rent, queues, and years.

*   [Think about home]
    You remember the heat of Khartoum, different from Cairo's heat. Cairo presses from all sides. Khartoum used to come down from the sky.
    
    You remember your father before the shell took him. You remember neighbours who became family, friends who scattered to Egypt, to Ethiopia, to graves no one can visit.
    
    Your Arabic still catches on certain words. "Ayna {gender == "male": enta | enti} min?" people ask. Where are you from?
    
    The question never stops being complicated.
    
    -> intro_sudanese_home_reflection

*   [Think about your mother]
    She adjusted better than you expected. Picked up the Egyptian dialect faster than you did. Found a network of Sudanese women who trade clinic names, charity kitchens, and landlord warnings like contraband.
    
    But her health is failing. Displacement, poor diet, and uncertainty wear on a body.
    
    She needs medication you can barely afford.
    
    -> intro_done

=== intro_sudanese_home_reflection ===
*   [Continue]
    Home is gone. Home is this cramped apartment in Imbaba, with its peeling paint, a salvaged cooling unit, and electricity that fails exactly when the tower district gets priority.
    
    Home is your mother, keeping her Sudanese ID in a plastic bag like a talisman.
    
    -> intro_done

=== intro_done ===
Your story begins in Cairo, 2060: a city where an autonomous taxi can cross the river, a drone can map a block before breakfast, and a platform can know your name before your neighbour does. None of that guarantees food, medicine, privacy, or a way out.

The day stretches before you, one more day in a city crowded far beyond what its water pipes, power lines, and shaded streets were built to carry. It can still find ways to make one {gender == "male": man | woman} feel cornered.

{ money < 40:
    Your wallet is already thin enough that one missed shift becomes a household emergency.
- else:
    You count the notes twice. There is room for one careful mistake, not two.
}

Survive.
-> DONE

=== background_medical_clinic ===
The smell of antiseptic, stale sweat, and bad fluorescent lighting lands in your body before it reaches your thoughts. For one ugly second, the clinic feels closer to the life you lost than any memory has managed in months.
-> DONE

=== background_prisoner_heat ===
You do not need anyone to explain why police notice you faster. Some names stay filed in the state even when the paper itself is out of sight.
-> DONE

=== background_sudanese_solidarity ===
The kindness comes in the way displaced people recognize one another without asking for a perfect explanation first. Someone presses food into your hand as if sparing you the embarrassment of needing it is part of the gift.
-> DONE
