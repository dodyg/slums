# SLUMS - Main Ink Story

VAR gender = "female"

INCLUDE crime.ink
INCLUDE events.ink
INCLUDE npcs.ink
INCLUDE endings.ink
INCLUDE weather.ink
INCLUDE seasons.ink
INCLUDE community.ink
INCLUDE debt.ink

=== intro_medical ===
Cairo, 2060.

The future arrived in layers. Autonomous electric cars whisper along the wealthy roads, electronic drones stitch delivery lanes through the sky, and portable nuclear units keep the new towers bright. Down here in Imbaba, the building shares a rationed microgrid with three neighbours. The rooftop relay drops out whenever the heat rises. A persistent AI lives in almost every cheap phone, listening for its owner's voice and selling a cleaner version of the truth to whoever can pay.

Cybernetics are everywhere in the advertisements and rare in the clinic. The rich replace failing joints before they fail. Your mother waits for a nurse with a cracked diagnostic tablet and a cupboard of ordinary medicine.

Three years of medical school.

Three years of dreaming of a white coat, of a stethoscope around your neck, of rounds at Kasr El Ainy instead of counting bus fare and pharmacy prices.

Then Baba died, and the tuition money evaporated with him.

Now you sit in your single room in Imbaba, listening to your mother cough into a washed-thin handkerchief while an autonomous EV shuttle glides past below and an old microbus argues with it for road space. The cough sounds wet. Worse than yesterday.

*   [Check on her]
    You kneel beside her mattress. Her forehead is clammy, her breathing labored.
    
    "Mama, we need to get you to a doctor."
    
    She waves you off weakly. "Doctors cost money, habibi. Money we don't have."
    
    You have some medical training. You could try to help her yourself, or you could spend what little you have on a proper consultation.
    -> intro_medical_check_mother

*   [Look for work instead]
    You can't help her if you can't pay for medicine.
    
    You step into the alley behind the building. The first ahwa chairs are scraping the pavement beneath the drone corridor. Oil hisses at the taameya cart while a neighbour tries to restart a salvaged service bot with a screwdriver. Somebody upstairs is already shouting about money.
    
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
Cairo, 2060. The city remembers everything that can be indexed.

The cameras are smaller now. Some ride on insect-sized electronic drones; some sit behind the eyes of public-service kiosks; some are buried in the persistent AI that manages every official account you cannot afford to lose. The state calls it safety. The people who live under it call it being watched from above, below, and inside the device in your pocket.

The cell door opened eight months ago.

Your mother aged ten years in the two you were inside. The neighbours still lower their voices when you pass. The amn el-dawla file never really closes. It just sits somewhere with your name on it, waiting.

You are twenty-six years old. You have a criminal record, a gap in your employment history, and a mother who needs medicine you can't afford.

*   [Think about what happened]
    It was just a protest. A gathering in Tahrir against predictive policing, rationed power, and the price of being misclassified by an AI that never has to meet you. Bread prices, corruption, humiliation, all the things that pile up in a chest until they come out as shouting.
    
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
Cairo, 2060. Above the roofs, delivery drones blink in ordered lanes. At street level, families still carry water by hand when the neighborhood reactor goes into safety mode.

You still dream of the Nile in Khartoum.

Before the jets came. Before the explosions that shook your apartment building at 3 AM. Before your mother grabbed you by the wrist and said, "We leave now. Take only what you can carry."

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
    
    But her health is failing. The stress of displacement, the poor diet, the uncertainty—it wears on a body.
    
    She needs medication you can barely afford.
    
    -> intro_done

=== intro_sudanese_home_reflection ===
*   [Continue]
    Home is gone. Home is this cramped apartment in Imbaba, with its peeling paint, a salvaged cooling unit, and electricity that fails exactly when the portable reactor reserve is diverted to the tower district.
    
    Home is your mother, keeping her Sudanese ID in a plastic bag like a talisman.
    
    -> intro_done

=== intro_done ===
Your story begins in Cairo, 2060: a city where an autonomous car can cross the river without a driver, a drone can map a whole block before breakfast, and a persistent AI can know your name before your neighbour does. None of that guarantees food, medicine, privacy, or a way out.

The day stretches before you, one more day in a city that can hold twelve million people and still find ways to make one {gender == "male": man | woman} feel cornered.

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
