# Recurring NPC scenes

=== landlord_rent_negotiation ===
Hajj Mahmoud is waiting in the Ard al-Liwa stairwell, prayer beads sliding through his fingers. He asks for the rent without raising his voice, which somehow makes it worse.

*   [Answer politely and ask for time]
	# STRESS:5
	# NPC_TRUST:LandlordHajjMahmoud,8
	# MESSAGE:Hajj Mahmoud gives you a little room to breathe.
	He mutters that sabr has limits, but he steps aside and lets the matter rest for today. For today only.
	-> DONE

*   [Answer defiantly]
	# STRESS:10
	# NPC_TRUST:LandlordHajjMahmoud,-12
	# MESSAGE:The exchange hardens. The rent feels heavier already.
	He says nothing for a moment. Then he nods once, the way men do when they decide to remember an insult longer than a debt.
	-> DONE

=== landlord_rent_negotiation_warm ===
Hajj Mahmoud looks tired more than angry. He reminds you that everyone in the building is bleeding money, then asks what you can manage this week.

*   [Answer politely and ask for time]
	# STRESS:5
	# NPC_TRUST:LandlordHajjMahmoud,12
	# MESSAGE:Hajj Mahmoud gives you a little room to breathe.
	He mutters that sabr has limits, but he steps aside and lets the matter rest for today. For today only.
	-> DONE

*   [Answer defiantly]
	# STRESS:10
	# NPC_TRUST:LandlordHajjMahmoud,-10
	# MESSAGE:The exchange hardens. The rent feels heavier already.
	He says nothing for a moment. Then he nods once, the way men do when they decide to remember an insult longer than a debt.
	-> DONE

=== landlord_rent_negotiation_hostile ===
Hajj Mahmoud does not invite excuses. His jaw tightens before you say a word and the whole stairwell feels as narrow as a coffin lid.

*   [Answer politely and ask for time]
	# STRESS:5
	# NPC_TRUST:LandlordHajjMahmoud,5
	# MESSAGE:Hajj Mahmoud gives you a little room to breathe.
	He mutters that sabr has limits, but he steps aside and lets the matter rest for today. For today only.
	-> DONE

*   [Answer defiantly]
	# STRESS:10
	# NPC_TRUST:LandlordHajjMahmoud,-18
	# MESSAGE:The exchange hardens. The rent feels heavier already.
	He says nothing for a moment. Then he nods once, the way men do when they decide to remember an insult longer than a debt.
	-> DONE

=== fixer_first_contact ===
Umm Karim watches the Imbaba market crowd instead of you. She says there are always errands for women who keep their mouths shut and their feet moving.

*   [Listen carefully]
	# NPC_TRUST:FixerUmmKarim,8
	# FACTION_REP:ImbabaCrew,6
	# FLAG:fixer_met
	# MESSAGE:Umm Karim decides you may be useful.
	She gives you no names and no promises. Only a time, a side street, and the warning that loose talk can finish a woman long before the police do.
	-> DONE

*   [Refuse and leave]
	# STRESS:-2
	# NPC_TRUST:FixerUmmKarim,-6
	# MESSAGE:You walk away, but the offer lingers in your mind.
	Umm Karim shrugs as if she expected nothing more. In Cairo, another desperate woman is always half a street away.
	-> DONE

=== fixer_repeat_contact ===
Umm Karim does not waste greetings. Somewhere behind her, a porter curses and a tuk-tuk horn answers him. She asks whether you came for real work or only stories.

*   [Ask for more serious work]
	# NPC_TRUST:FixerUmmKarim,5
	# FACTION_REP:ImbabaCrew,4
	# STRESS:4
	# MESSAGE:Umm Karim starts measuring you against tougher jobs.
	She says ambition is cheap and silence is expensive. Then she tells you where to stand after maghrib and who not to look at.
	-> DONE

*   [Keep it small for now]
	# NPC_TRUST:FixerUmmKarim,2
	# STRESS:-2
	# MESSAGE:You keep the conversation cautious.
	She smirks. Survival first, empire later.
	-> DONE

=== officer_checkpoint ===
Officer Khalid stops you near the Dokki square. His tone is almost casual, but his eyes keep inventory.

*   [Answer calmly]
	# STRESS:6
	# NPC_TRUST:OfficerKhalid,6
	# MESSAGE:Khalid lets you pass, though not warmly.
	He warns you to be home earlier and waves you on without apology, as if the pavement belongs to him.
	-> DONE

*   [Offer a small bribe]
	# MONEY:-15
	# STRESS:8
	# NPC_TRUST:OfficerKhalid,2
	# MESSAGE:The bill disappears as neatly as the conversation.
	He folds the note away with a face so blank it feels practiced down to the muscle.
	-> DONE

*   [Stay silent and hard]
	# STRESS:12
	# NPC_TRUST:OfficerKhalid,-6
	# MESSAGE:Silence keeps your pride intact, not your comfort.
	He keeps you there long enough to make the point, then lets you go under a stare that follows two steps after you move.
	-> DONE

=== officer_checkpoint_hot ===
Officer Khalid is no longer making conversation. He asks where you were last night and does not bother pretending the question is routine.

*   [Answer calmly]
	# STRESS:6
	# NPC_TRUST:OfficerKhalid,3
	# MESSAGE:Khalid lets you pass, though not warmly.
	He warns you to be home earlier and waves you on without apology, as if the pavement belongs to him.
	-> DONE

*   [Offer a small bribe]
	# MONEY:-15
	# STRESS:8
	# NPC_TRUST:OfficerKhalid,2
	# MESSAGE:The bill disappears as neatly as the conversation.
	He folds the note away with a face so blank it feels practiced down to the muscle.
	-> DONE

*   [Stay silent and hard]
	# STRESS:12
	# NPC_TRUST:OfficerKhalid,-12
	# MESSAGE:Silence keeps your pride intact, not your comfort.
	He keeps you there long enough to make the point, then lets you go under a stare that follows two steps after you move.
	-> DONE

=== neighbor_mona ===
Mona catches you on the landing with a bowl balanced on her palm and the whole building's news already alive in her eyes.

*   [Trade news and ask how people are coping]
	# STRESS:-4
	# NPC_TRUST:NeighborMona,6
	# MESSAGE:Mona shares what she knows and starts looking out for you.
	She tells you which landlord is shouting, which grocer still gives credit, and which alley to avoid after isha. In a place like this, information is almost a meal.
	-> DONE

*   [Keep the talk short and guarded]
	# STRESS:1
	# NPC_TRUST:NeighborMona,-4
	# MESSAGE:Mona notices the wall you put up.
	She nods and steps aside, but the warmth drains out of the stairwell with the conversation.
	-> DONE

=== neighbor_mona_warm ===
Mona has already set aside a chipped glass of tea for you. She lowers her voice before mentioning the latest trouble in the building.

*   [Ask her to warn you if the landlord comes early]
	# NPC_TRUST:NeighborMona,8
	# STRESS:-5
	# MESSAGE:Mona agrees to keep an eye out for you.
	She says no one survives Cairo alone, then presses the tea into your hand like an oath.
	-> DONE

*   [Tell her you do not want favors]
	# NPC_TRUST:NeighborMona,-6
	# STRESS:3
	# MESSAGE:Mona backs off, hurt more than angry.
	She murmurs "maashi" and busies herself with the laundry line, leaving you with your pride and nothing else.
	-> DONE

=== nurse_salma ===
Nurse Salma is moving faster than the clinic can hold. She checks names, calms a crying child off-screen, and still finds a second to look straight at you.

*   [Ask about extra shifts]
	# NPC_TRUST:NurseSalma,6
	# STRESS:2
	# MESSAGE:Salma tells you to come back before zuhr if you want work.
	She says clinics always need hands that can read a form, keep a line moving, and stay steady when everyone else is tired.
	-> DONE

*   [Ask quietly about cheap medicine for your mother]
	# NPC_TRUST:NurseSalma,8
	# MESSAGE:Salma points you toward the cheapest pharmacy she trusts.
	Her voice drops. "Do not buy from the man by the bridge. His prices change with your face."
	-> DONE

=== nurse_salma_warm ===
Salma sees you and immediately shifts two paper files off the only free chair. In this clinic, that counts as kindness.

*   [Ask what work she trusts you with now]
	# NPC_TRUST:NurseSalma,7
	# STRESS:-2
	# MESSAGE:Salma starts treating you like part of the clinic's rhythm.
	She tells you which doctor is impossible, which patients need patience, and how to survive a day without letting the panic in the waiting room into your bones.
	-> DONE

*   [Ask for advice about your mother's health]
	# NPC_TRUST:NurseSalma,5
	# MESSAGE:Salma gives you practical advice without wasting words.
	She writes a dosage reminder on scrap paper and tells you what warning signs mean you cannot wait another day.
	-> DONE

=== abu_samir ===
Abu Samir stands in the workshop doorway counting finished pieces without looking up. The room behind him smells of hot fabric and burnt steam.

*   [Ask for a shift]
	# NPC_TRUST:WorkshopBossAbuSamir,6
	# STRESS:3
	# MESSAGE:Abu Samir tells you to show up early if you want the better table.
	He says speed matters, silence matters more, and pay will always disappoint you no matter how hard you work.
	-> DONE

*   [Complain about the rates]
	# NPC_TRUST:WorkshopBossAbuSamir,-8
	# STRESS:6
	# MESSAGE:Abu Samir takes the complaint personally.
	He snorts that every girl wants cleaner money than the city has to offer and asks whether you came to work or to lecture him.
	-> DONE

=== abu_samir_warm ===
Abu Samir jerks his chin toward an empty stool before you even ask. That is as close as he comes to saying he was expecting you.

*   [Ask if he can keep a shift open this week]
	# NPC_TRUST:WorkshopBossAbuSamir,7
	# STRESS:-2
	# MESSAGE:Abu Samir decides you are reliable enough to schedule.
	He warns you not to make him regret it, but the warning lands softer than usual.
	-> DONE

*   [Ask him about the neighborhood rumors]
	# NPC_TRUST:WorkshopBossAbuSamir,3
	# MESSAGE:Abu Samir shares only what he thinks is useful.
	He tells you which supplier is cheating, which street got hotter with police, and which promises are worth less than thread.
	-> DONE

=== nadia_cafe ===
Nadia runs Ahwa El-Galaa from behind a cloud of steam and sharp memory. She knows who pays late, who tips, and who lies about both.

*   [Ask if she needs another pair of hands]
	# NPC_TRUST:CafeOwnerNadia,6
	# STRESS:1
	# MESSAGE:Nadia tells you to come at the evening rush if you are serious.
	She says carrying trays is easy. Reading men before they decide not to pay is the real skill.
	-> DONE

*   [Stay for gossip and listen]
	# NPC_TRUST:CafeOwnerNadia,4
	# STRESS:-3
	# MESSAGE:Nadia feeds you talk as useful as any meal.
	Over sweet tea she sketches the day's map of who is desperate, who is hiring, and who has started attracting police attention.
	-> DONE

=== nadia_cafe_warm ===
Nadia slides a glass of tea toward you without asking for payment first. Around her, the ahwa rattles with spoons, football arguments, and bad decisions.

*   [Ask her to put your name in for steady shifts]
	# NPC_TRUST:CafeOwnerNadia,8
	# STRESS:-2
	# MESSAGE:Nadia starts mentioning you when casual work comes up.
	She says reliability is rarer than charm and worth more in the long run.
	-> DONE

*   [Ask what people are saying about the streets tonight]
	# NPC_TRUST:CafeOwnerNadia,3
	# MESSAGE:Nadia gives you the version of the truth that keeps women safe.
	She names the corners to avoid after dark and the kind of smile that should make you turn around immediately.
	-> DONE

=== hanan_fence ===
Hanan stands half inside the market crowd and half outside it, watching hands, pockets, and faces with the calm of someone who makes a living off bad timing.

*   [Ask what kind of goods move quietly this week]
	# NPC_TRUST:FenceHanan,6
	# FACTION_REP:ImbabaCrew,3
	# MESSAGE:Hanan gives you a careful answer and measures your discretion.
	She says the smart girls deal in things that look ordinary from a distance and forgettable up close.
	-> DONE

*   [Ask for easy money]
	# NPC_TRUST:FenceHanan,-5
	# STRESS:4
	# MESSAGE:Hanan does not like desperation announced out loud.
	She tells you easy money is what men call a trap after they step into it.
	-> DONE

=== hanan_fence_warm ===
Hanan does not smile, but she does make room for you under the shade cloth. In the market, that is almost the same thing.

*   [Ask what heat is building in Imbaba]
	# NPC_TRUST:FenceHanan,5
	# MESSAGE:Hanan points out which streets are getting too curious.
	She names a kiosk, a side alley, and two boys asking the wrong questions for someone else's benefit.
	-> DONE

*   [Ask whether she trusts you with more delicate work]
	# NPC_TRUST:FenceHanan,7
	# FACTION_REP:ImbabaCrew,4
	# STRESS:2
	# MESSAGE:Hanan starts considering you for riskier errands.
	She reminds you that profit is never the full price, only the part fools count first.
	-> DONE

=== youssef_runner ===
Youssef drifts along the edge of Midan Al-Tahrir with the restless energy of a man who carries messages he would never write down.

*   [Ask what the police are focusing on tonight]
	# NPC_TRUST:RunnerYoussef,5
	# STRESS:-1
	# MESSAGE:Youssef shares what he has heard about checkpoints.
	He says uniforms are less dangerous than the men in plain shirts pretending not to listen.
	-> DONE

*   [Ask if anyone in Dokki is hiring for dirty errands]
	# NPC_TRUST:RunnerYoussef,4
	# FACTION_REP:DokkiThugs,3
	# MESSAGE:Youssef hints at the kind of work that always costs more than it pays.
	He tells you where to stand if you insist, then adds that regret arrives faster in Dokki because the pavements are smoother.
	-> DONE

=== youssef_runner_hot ===
Youssef spots you before you speak and checks the street behind you first. His voice stays low enough to disappear into the traffic.

*   [Ask whether your name is moving around]
	# NPC_TRUST:RunnerYoussef,3
	# STRESS:4
	# MESSAGE:Youssef admits the street is talking.
	He says your face has started sticking in other people's memory, and that is usually how trouble becomes a schedule.
	-> DONE

*   [Tell him to forget he saw you]
	# NPC_TRUST:RunnerYoussef,-6
	# STRESS:2
	# MESSAGE:Youssef decides distance is safer.
	He lifts both hands, steps back into the crowd, and leaves you with the certainty that isolation is its own kind of warning.
	-> DONE