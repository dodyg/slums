# Crime reactive scenes

=== crime_first_success ===
The cash in your pocket feels warmer than it should. It smells faintly of sweat and street dust. Cairo rewards nerve, but it memorizes faces.

*   [Hide the money and go home]
    You fold the notes flat, press them deep in your pocket, and walk home by the longest route — through three different alleys, past four different ahwas, avoiding every face that might have been watching. The money is real. The fear is realer.
    # STRESS:5
    # MESSAGE:The first successful crime leaves your nerves rattling.
    -> DONE

*   [Buy something for your mother on the way home]
    You stop at the pharmacy and pick up the heart medication she has been rationing for a week. The pharmacist does not ask where the money came from. In Cairo, pharmacies do not question the origin of cash any more than the buildings question the origin of rent.
    # STRESS:3
    # MONEY:-15
    # MOTHER_HEALTH:5
    # MESSAGE:Your first crime pays for your mother's medication. The math is ugly but it works.
    -> DONE

*   [Sit with the guilt for a while]
    You find a bench near the bus station and hold the money in your hand like a question. It is not much. It is enough to matter. The city moves around you, indifferent and efficient, and you sit in the middle of it holding proof that the line you promised not to cross has already been crossed.
    # STRESS:8
    # FLAG:first_crime_reflected
    # MESSAGE:You sit with the money and the guilt. The first crime changes something that cannot be changed back.
    -> DONE

=== crime_warning ===
By maghrib, the talk on the street has already turned. Men lower their voices when you pass. A kiosk boy looks at you too long. Too many questions are being asked in too casual a tone.

*   [Lay low for a few days]
    You change your route home, avoid the usual corners, and keep your eyes on the ground in the market. The street forgets quickly, but not instantly, and the difference between those two speeds is where the danger lives.
    # STRESS:5
    # MESSAGE:The street is watching. You keep your head down until the attention moves on.
    -> DONE

*   [Ask Umm Karim to gauge the heat]
    Umm Karim listens without looking at you. She says the heat is real but not personal yet — they are watching the neighbourhood, not hunting one face. She tells you to stay off the main routes for two days and to stop looking nervous, because nervous is its own kind of confession.
    # STRESS:4
    # NPC_TRUST:FixerUmmKarim,2
    # MESSAGE:Umm Karim reads the street for you. The heat is general, not specific — for now.
    -> DONE

*   [Push through — you cannot afford to lose days]
    You refuse to let fear reorganize your life. The street watches, but the street always watches, and you have been visible before. You keep moving, keep working, keep your face set to the particular blankness that Cairo teaches its women as a first line of defense.
    # STRESS:10
    # MESSAGE:You refuse to hide. The street takes note of the defiance and files it alongside everything else.
    -> DONE

=== crime_police_encounter ===
The patrol appears at the corner where the alley meets the main road. Two uniforms and a plainclothes officer who is watching faces with the patience of a man who has all day and no particular target. Your pulse spikes. You have done nothing wrong today, but in Cairo, "nothing wrong" is not the same as "nothing to hide."

*   [Walk past normally]
    You force your shoulders to relax, your pace to stay even, your eyes to look bored instead of frightened. The plainclothes officer glances at you, then past you, then back at the stream of faces. You round the corner and do not start breathing again until you are half a block away.
    # STRESS:6
    # MESSAGE:A police patrol scans the corner. You walk through it without breaking stride.
    -> DONE

*   [Take a detour through the side streets]
    You duck into the nearest alley and navigate three blocks of back routes that add ten minutes to your walk but subtract the risk of being remembered. The detour takes you past the back of the ahwa where Nadia is dumping dishwater. She raises an eyebrow but says nothing.
    # STRESS:4
    # ENERGY:-3
    # MESSAGE:You avoid the patrol through back alleys. The detour costs time but saves risk.
    -> DONE

=== crime_gang_retaliation ===
The message is not written. It is left: your door scratched, a glass bottle shattered at your threshold, and the particular silence in the stairwell that means someone has been here who does not usually visit. The building holds its breath.

Mona finds you in the morning and says three men were asking about you last night. They did not knock. They only stood at the bottom of the stairs and looked up.

*   [Reach out to Umm Karim for protection]
    You send word through the market chain — two intermediaries, no names, the standard protocol for asking a favour that cannot be asked directly. Umm Karim's reply comes back within the hour: she will speak to them, but the conversation has a price, and the price is not money.
    # STRESS:6
    # NPC_TRUST:FixerUmmKarim,3
    # FACTION_REP:ImbabaCrew,5
    # MESSAGE:Gang retaliation threatens your home. Umm Karim offers to negotiate — for a price.
    -> DONE

*   [Go to the gang directly]
    You find the nearest Imbaba crew corner and ask to speak with whoever sent the men to your building. The conversation is short, tense, and conducted entirely in the language of implication. You agree to pay a percentage. They agree to call off the pressure. Neither of you trusts the other, but in Cairo, distrust is its own kind of contract.
    # STRESS:8
    # MONEY:-25
    # FACTION_REP:ImbabaCrew,3
    # MESSAGE:You confront the gang directly. The deal costs money and dignity but buys time.
    -> DONE

*   [Leave the apartment for a few nights]
    You pack a bag, tell Mona you are staying with a cousin, and sleep on the floor of a friend's apartment two districts away. Your mother stays behind because she cannot travel, and the distance between you and the building is measured in worry as much as kilometers.
    # STRESS:10
    # ENERGY:-5
    # MESSAGE:You flee the apartment for a few nights. Your mother stays behind, and the separation weighs on you.
    -> DONE

=== crime_hanan_cover ===
Hanan never admits she helped. She only mutters that market people survive by deciding what to notice and what to forget, then tells you not to make her spend that kind of effort twice in one week.
# STRESS:-2
# MESSAGE:Hanan's cover buys you a little breathing room.
-> DONE

=== crime_youssef_tipoff ===
Youssef catches you half a block too early and changes your route with a glance and two clipped words. In Cairo, sometimes warning is the only mercy anyone can afford.
# STRESS:-1
# MESSAGE:Youssef gets word to you before the dragnet tightens.
-> DONE

=== crime_hanan_salvage ===
Later, Hanan presses a folded note into your palm and tells you not to ask which part of the mess she managed to sell. In her world, survival counts cleaner than explanations.
# STRESS:-2
# MESSAGE:Hanan claws a little value back out of the failure.
-> DONE

=== crime_youssef_escape ===
Youssef keeps you moving until your breathing stops sounding guilty. Only then does he say that a woman who freezes in Dokki gets remembered by too many people at once.
# STRESS:-2
# MESSAGE:Youssef turns a bad night into a survivable one.
-> DONE

=== crime_hanan_fence_success ===
Hanan takes the wrapped bundle without looking eager. She tests one seam, nods once, and says the real profit came from being forgettable in a place where everyone watches everyone else.
# STRESS:2
# MESSAGE:Hanan's route pays because it stays quiet.
-> DONE

=== crime_hanan_fence_detected ===
The money lands in your hand, but so does the look from a porter who will remember your face tomorrow. Hanan clicks her tongue and tells you quiet work only counts when it stays quiet.
# STRESS:5
# MESSAGE:The market route pays, but your name circulates with it.
-> DONE

=== crime_hanan_fence_failure ===
Hanan barely touches the goods before pushing them back. Too hot, too obvious, too many eyes already on the wrong corners. She tells you failed work in the market becomes gossip faster than smoke.
# STRESS:4
# MESSAGE:Hanan refuses to move the bad bundle.
-> DONE

=== crime_youssef_drop_success ===
Youssef's instructions are clipped and exact: the side entrance, the service lift, the handoff without conversation. In Dokki, clean shoes and good timing hide more than alley shadows ever could.
# STRESS:3
# MESSAGE:The Dokki drop works because you follow the route exactly.
-> DONE

=== crime_youssef_drop_detected ===
The handoff lands, but a building guard watches one second too long and a patrol car idles where it should have kept moving. Youssef says the city took its cut the moment someone decided to remember you.
# STRESS:7
# MESSAGE:The Dokki drop succeeds, but it leaves a trail of attention.
-> DONE

=== crime_youssef_drop_failure ===
The timing slips. A door stays shut, a messenger never appears, and suddenly you are only a young woman standing in the wrong corridor with no story that sounds clean enough. Youssef mutters that Dokki punishes hesitation twice.
# STRESS:6
# MESSAGE:The Dokki drop collapses when the timing turns against you.
-> DONE

=== crime_ummkarim_errand_success ===
Umm Karim does not praise you. She only says the people above her noticed you delivered without panic, which in Cairo is closer to a recommendation than warmth. The money feels heavier because it came tied to names you still do not know.
# STRESS:4
# MESSAGE:Umm Karim's network starts treating you like usable labor.
-> DONE

=== crime_ummkarim_errand_detected ===
You finish the errand, but by the end of the night two different men have looked at you as if measuring whether they will need your face later. Umm Karim says higher work pays better because every mistake arrives with witnesses.
# STRESS:8
# MESSAGE:The network errand pays, but people higher up now know you by sight.
-> DONE

=== crime_ummkarim_errand_failure ===
By the time you realize the route has turned wrong, the mistake is already public to the only people who matter. Umm Karim does not raise her voice. She just tells you that dangerous work stops being anonymous the moment it fails.
# STRESS:7
# MESSAGE:Umm Karim makes it clear that failed network work carries a memory.
-> DONE

=== crime_safaa_reroute ===
Safaa does not ask what you did. She only snaps out a different route, three names to avoid, and a warning that depots remember panic louder than guilt.
# STRESS:-2
# MESSAGE:Safaa pushes the heat onto a different line before it settles on you.
-> DONE

=== crime_safaa_salvage ===
Later, Safaa presses a few crumpled notes into your hand and says missed money is better than police memory. At the depot, that counts as mercy.
# STRESS:-2
# MESSAGE:Safaa claws a little value back out of the mess.
-> DONE

=== crime_iman_cover ===
Iman notices the attention in the lane before you do. She jerks her chin toward the back stairs and tells a customer to complain louder, giving the street something else to look at.
# STRESS:-2
# MESSAGE:Iman gives you a narrow exit and wastes none of it.
-> DONE

=== crime_iman_exit ===
Iman shoves a damp apron into your hands, tells you to keep walking, and lets the lane mistake you for tired labor instead of trouble.
# STRESS:-2
# MESSAGE:Iman gets you clear before the wrong story hardens around you.
-> DONE

=== crime_safaa_skim_success ===
The depot is chaos anyway, which is what makes the skim possible. A little money disappears between shouted fares, bad tempers, and the next bus lurching forward before anyone can count twice.
# STRESS:3
# MESSAGE:The depot skim works because nobody in the yard has time to untangle every hand.
-> DONE

=== crime_safaa_skim_detected ===
You get the money, but so does a driver's narrowed look. Safaa mutters that depots forgive noise, not missing cash, and tells you to stay out of sight until the next route turns over.
# STRESS:6
# MESSAGE:The depot skim pays, but somebody in the yard is doing the math.
-> DONE

=== crime_safaa_skim_failure ===
A fare count comes up wrong at exactly the wrong moment. The driver shouts, passengers start listening, and suddenly the whole depot feels small enough to trap you in one bad minute.
# STRESS:6
# MESSAGE:The depot skim collapses when the count stops matching the noise.
-> DONE

=== crime_iman_bundle_success ===
The switch works because laundry work already looks like repetition: the same motions, the same tags, the same bundles passing from hand to hand until one of them becomes yours instead.
# STRESS:2
# MESSAGE:The bundle lift works because routine hides the theft better than speed.
-> DONE

=== crime_iman_bundle_detected ===
The lifted bundle clears the door, but a customer comes back too quickly and starts describing cloth in the dangerous, exact way only angry people do. The lane turns attentive all at once.
# STRESS:5
# MESSAGE:The bundle lift pays, but Shubra starts paying attention back.
-> DONE

=== crime_iman_bundle_failure ===
The ticket numbers stop making sense, the wrong woman starts shouting, and every pressed shirt in the shop suddenly feels like evidence. In a place this tight, failure has nowhere to disperse.
# STRESS:5
# MESSAGE:The bundle lift fails when the tags refuse to line up.
-> DONE