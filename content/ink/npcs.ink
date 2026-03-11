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

=== fixer_recent_refusal ===
Umm Karim remembers the last time you walked away before she has finished her sentence. She does not sound offended. She sounds like someone checking whether your fear has matured into discipline.

*   [Admit you misread what she was offering]
	# NPC_TRUST:FixerUmmKarim,4
	# MESSAGE:Umm Karim decides honesty is slightly less useless than panic.
	She says there is always another errand, but never for women who waste other people's time twice.
	-> DONE

*   [Stay guarded and ask only for small work]
	# NPC_TRUST:FixerUmmKarim,1
	# REFUSAL:FixerUmmKarim
	# MESSAGE:You keep the conversation narrow, and Umm Karim notices.
	She nods once, as if filing you under a smaller category than before.
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

=== neighbor_mona_helped ===
Mona does not pretend she forgot the last time you stepped in when the building talk turned ugly. The stairwell feels less like a hallway and more like a line being held.

*   [Ask whether anyone needs anything this week]
	# NPC_TRUST:NeighborMona,6
	# HELPED:NeighborMona,true
	# MESSAGE:Mona starts treating you like someone the building can count on.
	She tells you whose gas ran out, whose son is looking for a messenger shift, and which landlord is threatening to raise rent again.
	-> DONE

*   [Tell her you only did what anyone should do]
	# NPC_TRUST:NeighborMona,3
	# MESSAGE:Mona hears the modesty and files it as another kind of decency.
	She says Cairo would be easier if more people spoke like that and meant it.
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

=== nurse_salma_debt ===
Salma does not mention the medicine she covered for your mother, but the omission makes the debt feel more exact, not less.

*   [Promise you will settle it when you can]
	# NPC_TRUST:NurseSalma,4
	# DEBT:NurseSalma,true
	# FAVOR:NurseSalma
	# MESSAGE:Salma accepts the promise without pretending promises are cash.
	She says repayment matters less than whether you become the kind of woman who vanishes when help gets expensive.
	-> DONE

*   [Ask what she needs from you now]
	# NPC_TRUST:NurseSalma,6
	# DEBT:NurseSalma,true
	# FAVOR:NurseSalma
	# MESSAGE:Salma starts treating the debt like a test of reliability instead of gratitude.
	She points at the waiting room and says there is always work if you mean to pay in effort first.
	-> DONE

=== nurse_salma_suspicious ===
Salma watches your hands before she answers your question. The clinic has taught her how stress looks when it comes from hunger, grief, guilt, or all three at once.

*   [Insist work is only work and the rest is your business]
	# NPC_TRUST:NurseSalma,-5
	# REFUSAL:NurseSalma
	# MESSAGE:Salma pulls back and leaves the conversation strictly practical.
	She says the clinic is full of women carrying trouble. The clever ones do not drag it into the triage line.
	-> DONE

*   [Admit you are trying to keep too many lives apart]
	# NPC_TRUST:NurseSalma,2
	# MESSAGE:Salma does not approve, but she decides honesty is worth something.
	She tells you exhaustion makes liars sloppy and patients pay for other people's sloppiness first.
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

=== abu_samir_embarrassed ===
Abu Samir does not raise his voice about the last mistake. The way he moves your stool to the side says enough on its own.

*   [Own the mistake and ask for another chance]
	# NPC_TRUST:WorkshopBossAbuSamir,3
	# EMBARRASSED:WorkshopBossAbuSamir,false
	# MESSAGE:Abu Samir agrees to another chance, but not a forgiving one.
	He says cloth forgives nothing and neither do men running on margins this thin.
	-> DONE

*   [Act like he is overreacting]
	# NPC_TRUST:WorkshopBossAbuSamir,-7
	# EMBARRASSED:WorkshopBossAbuSamir,true
	# MESSAGE:Abu Samir decides you have not learned the part that matters.
	He turns away before you finish speaking, which in his workshop is worse than shouting.
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

=== nadia_cafe_double_life ===
Nadia studies the tiredness in your face like she is pricing it. The ahwa sees every kind of woman, but it remembers the ones trying to pass as two people at once.

*   [Say you are just tired from work]
	# NPC_TRUST:CafeOwnerNadia,-4
	# MESSAGE:Nadia lets the lie go by, which is not the same as believing it.
	She says women who come in carrying two stories usually lose track of which one is supposed to save them.
	-> DONE

*   [Ask whether she has ever kept two lives apart herself]
	# NPC_TRUST:CafeOwnerNadia,4
	# MESSAGE:Nadia answers with more sympathy than comfort.
	She says every woman in Cairo splits herself somehow. The trick is deciding which half gets to survive.
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

=== mariam_pharmacy ===
Mariam is labeling a shelf faster than the queue can empty. She keeps one eye on the stock sheet and one on the women pretending not to count their money twice.

*   [Ask whether she needs help on a shift]
	# NPC_TRUST:PharmacistMariam,6
	# STRESS:-1
	# MESSAGE:Mariam tells you when the cheaper deliveries usually land.
	She says the work is simple until the shortages start. After that, everybody wants mercy priced like a generic.
	-> DONE

*   [Ask how women are managing medicine costs]
	# NPC_TRUST:PharmacistMariam,5
	# MESSAGE:Mariam points out which brands are honest and which counters exploit panic.
	Her voice stays low. "Buy early if you can. By evening, every story becomes a surcharge."
	-> DONE

=== mariam_pharmacy_warm ===
Mariam nudges a stool out with her foot before you ask. In a pharmacy this cramped, making space is its own kind of trust.

*   [Ask if she can keep you in mind for steadier shifts]
	# NPC_TRUST:PharmacistMariam,7
	# STRESS:-2
	# MESSAGE:Mariam starts treating you like someone who can hold the counter together.
	She says reliable women are rarer than stock and worth more.
	-> DONE

*   [Ask what medicine your mother should never run out of]
	# NPC_TRUST:PharmacistMariam,4
	# MESSAGE:Mariam gives you the answer without theatrics.
	She circles two cheap options on scrap cardboard and tells you which substitute is false economy.
	-> DONE

=== safaa_depot ===
Safaa stands above the depot noise like she has decided volume is a tool, not a mood. Drivers argue, engines cough, and she keeps the line moving anyway.

*   [Ask whether she needs another pair of hands]
	# NPC_TRUST:DispatcherSafaa,6
	# STRESS:2
	# MESSAGE:Safaa tells you to come before the first real crush if you want work.
	She says the depot rewards lungs, timing, and the nerve to stop men wasting everybody's morning.
	-> DONE

*   [Ask which routes are worst this week]
	# NPC_TRUST:DispatcherSafaa,4
	# MESSAGE:Safaa gives you the honest answer because it saves time.
	She names the longest routes, the drivers who cheat on counts, and the hour when tempers become part of the fare.
	-> DONE

=== safaa_depot_warm ===
Safaa waves you closer without taking her eyes off the board. The trust is small, but at the depot small trust is still expensive.

*   [Ask if she will put you on the better line]
	# NPC_TRUST:DispatcherSafaa,7
	# STRESS:-1
	# MESSAGE:Safaa starts slotting you where chaos pays a little better.
	She says the cleaner line is never truly clean, only less stupid.
	-> DONE

*   [Ask what she hears from the streets before everyone else]
	# NPC_TRUST:DispatcherSafaa,3
	# MESSAGE:Safaa shares the kind of rumors that arrive before the police do.
	She tells you transport hears the city breathe before anyone else notices the chest tightening.
	-> DONE

=== iman_laundry ===
Iman runs the laundry with forearms shining from steam and a ledger balanced on the counter. Shirts hang overhead like surrendered flags.

*   [Ask if she needs help with the pressing]
	# NPC_TRUST:LaundryOwnerIman,6
	# STRESS:1
	# MESSAGE:Iman tells you which hours burn the worst and pay the same.
	She says laundry is honest work if you can stand heat, repetition, and customers who lie about stains.
	-> DONE

*   [Ask how business is holding up]
	# NPC_TRUST:LaundryOwnerIman,4
	# STRESS:-2
	# MESSAGE:Iman gives you the arithmetic nobody advertises.
	She says every family cuts something first, and pressed shirts disappear right after dignity starts getting expensive.
	-> DONE

=== iman_laundry_warm ===
Iman hands you a glass of water before the question is even finished. In the laundry, kindness arrives in practical forms.

*   [Ask if she trusts you with the front counter now]
	# NPC_TRUST:LaundryOwnerIman,7
	# STRESS:-2
	# MESSAGE:Iman starts leaving the better bundles in your hands.
	She says mistakes still cost money, but at least yours no longer feel guaranteed.
	-> DONE

*   [Ask what families are cutting back on first]
	# NPC_TRUST:LaundryOwnerIman,3
	# MESSAGE:Iman answers like someone who has watched decline item by item.
	She says starch, then special handling, then repairs, and finally the small courtesies that make poverty look less obvious.
	-> DONE

=== landlord_rent_broke ===
Hajj Mahmoud does not need to ask whether the week has gone badly. He can see it in your face before money becomes a subject either of you can pretend is theoretical.

*   [Admit you are short and ask for a few days]
	# STRESS:6
	# NPC_TRUST:LandlordHajjMahmoud,6
	# MESSAGE:Hajj Mahmoud believes the shortage, not the promise.
	He says hunger is common, excuses are commoner, and he is tired of sorting one from the other by stairwell light.
	-> DONE

*   [Pretend you have a plan already]
	# STRESS:8
	# NPC_TRUST:LandlordHajjMahmoud,-8
	# MESSAGE:Hajj Mahmoud hears the bluff and likes it even less than the debt.
	He tells you plans are what poor tenants call money before it arrives.
	-> DONE

=== fixer_trusted_operator ===
Umm Karim speaks to you without the old testing tone. That is not warmth. It is simply what trust sounds like in a line of work that charges extra for mistakes.

*   [Ask what kind of work requires steadier nerves now]
	# NPC_TRUST:FixerUmmKarim,4
	# FACTION_REP:ImbabaCrew,4
	# MESSAGE:Umm Karim starts discussing work as if you might survive it properly.
	She says the worst errands are never the loud ones. They are the ones that look ordinary while they ruin you.
	-> DONE

*   [Ask how women stay useful without becoming disposable]
	# NPC_TRUST:FixerUmmKarim,3
	# STRESS:-2
	# MESSAGE:Umm Karim answers more honestly than kindly.
	She says usefulness buys time, never safety, and women who confuse the two die confused.
	-> DONE

=== officer_checkpoint_marked ===
Officer Khalid already knows your face when he stops you. The recognition lands harder than a stranger's suspicion ever could.

*   [Answer carefully and keep your eyes level]
	# STRESS:7
	# NPC_TRUST:OfficerKhalid,2
	# MESSAGE:Khalid lets you go, but not back into anonymity.
	He says routine gets easier when women stop giving him reasons to remember them.
	-> DONE

*   [Ask what exactly he thinks he remembers]
	# STRESS:11
	# NPC_TRUST:OfficerKhalid,-5
	# MESSAGE:The question hardens the stop immediately.
	He smiles without humor and tells you memory is his business, not yours.
	-> DONE

=== neighbor_mona_lean_week ===
Mona sees the arithmetic on your face and lowers her voice before she says anything. Pride and hunger both echo badly in stairwells.

*   [Let her speak plainly]
	# NPC_TRUST:NeighborMona,5
	# STRESS:-3
	# MESSAGE:Mona stops pretending politeness is more useful than honesty.
	She tells you which flat still shares lentils, which grocer might extend two days of credit, and who in the building has started slipping.
	-> DONE

*   [Brush it off and say the week is fine]
	# NPC_TRUST:NeighborMona,-4
	# STRESS:2
	# MESSAGE:Mona hears the pride and knows better than to fight it.
	She only says "rabena yostor" and leaves the rest unsaid between you.
	-> DONE

=== nurse_salma_urgent ===
Salma takes one look at your expression and stops wasting words. In the clinic, urgency has its own dialect and she is already speaking it.

*   [Describe your mother's symptoms exactly]
	# NPC_TRUST:NurseSalma,6
	# MESSAGE:Salma answers like someone trying to buy you time with precision.
	She writes down what matters first, what can wait, and which sign means you stop bargaining with fate and come straight back.
	-> DONE

*   [Ask what you can manage cheaply at home]
	# NPC_TRUST:NurseSalma,4
	# STRESS:2
	# MESSAGE:Salma hears the money problem and does not pretend it is separate from medicine.
	She gives you the cheapest safe path she can without lying about how narrow it is.
	-> DONE

=== abu_samir_cold ===
Abu Samir does not look up immediately. In his workshop, that delay is a verdict before the words arrive.

*   [Ask whether he is still holding the last mistake against you]
	# NPC_TRUST:WorkshopBossAbuSamir,2
	# MESSAGE:Abu Samir answers with the kind of honesty reserved for people already in bad standing.
	He says margins remember everything because men like him cannot afford mercy as a management style.
	-> DONE

*   [Keep it strictly to work and rates]
	# NPC_TRUST:WorkshopBossAbuSamir,-3
	# STRESS:3
	# MESSAGE:Abu Samir accepts the coldness and returns it.
	He quotes the rate like he is discussing thread count with no human beings involved.
	-> DONE

=== nadia_cafe_cold ===
Nadia is civil, which is worse than warm by exactly the amount it takes to notice the missing softness.

*   [Ask whether she still has shifts worth chasing]
	# NPC_TRUST:CafeOwnerNadia,2
	# MESSAGE:Nadia answers professionally and nothing more.
	She says work exists. Whether she thinks of you when it appears is a separate matter.
	-> DONE

*   [Stay for tea and see if the room softens]
	# NPC_TRUST:CafeOwnerNadia,-2
	# STRESS:2
	# MESSAGE:Nadia does not make a scene of the distance.
	She lets the tea arrive and keeps the better conversation moving around you instead of through you.
	-> DONE

=== hanan_fence_cold ===
Hanan does not bother pretending you are a promising conversation. In the market, disinterest is often a more expensive warning than anger.

*   [Ask what made her stop trusting your judgment]
	# NPC_TRUST:FenceHanan,2
	# MESSAGE:Hanan answers because bluntness is cheaper than drama.
	She says discretion is not a mood, it is a discipline, and too many people perform it badly.
	-> DONE

*   [Ask whether any work is still worth discussing]
	# NPC_TRUST:FenceHanan,-3
	# STRESS:3
	# MESSAGE:Hanan leaves the door open only the width of a warning.
	She says there is always work. The question is whether she wants it standing next to her.
	-> DONE

=== youssef_runner_embedded ===
Youssef talks to you like someone already inside the edge of the route map. That is not friendship. It is simply the point where shared risk starts sounding familiar.

*   [Ask which routes are burning too bright now]
	# NPC_TRUST:RunnerYoussef,4
	# STRESS:-1
	# MESSAGE:Youssef gives you the kind of warning he does not waste on tourists.
	He says some streets are only dangerous after you stop treating them that way.
	-> DONE

*   [Ask how long anyone lasts doing this kind of work]
	# NPC_TRUST:RunnerYoussef,3
	# MESSAGE:Youssef answers with the honesty of someone too tired to posture.
	He says long enough to think they have learned the city, never long enough to finish learning it.
	-> DONE

=== mariam_pharmacy_urgent ===
Mariam hears the urgency before the details are finished. Her hands keep moving, but her attention narrows the way it does around the words that actually matter.

*   [Tell her exactly what your mother is running out of]
	# NPC_TRUST:PharmacistMariam,5
	# MESSAGE:Mariam starts sorting options by danger, not by price.
	She tells you what cannot wait, what can be stretched, and which cheap substitute is a false bargain.
	-> DONE

*   [Ask what you can buy first if you cannot cover everything]
	# NPC_TRUST:PharmacistMariam,4
	# STRESS:1
	# MESSAGE:Mariam gives you the triage answer, not the comforting one.
	She says poverty always forces priorities and medicine simply makes the cruelty easier to measure.
	-> DONE

=== safaa_depot_regular ===
Safaa barely looks surprised to see you. In the depot, being expected is its own kind of promotion.

*   [Ask what part of the yard is running badly today]
	# NPC_TRUST:DispatcherSafaa,4
	# MESSAGE:Safaa talks to you like another moving part in the machine.
	She says the worst hours are the ones where everybody is technically working and nothing is actually moving.
	-> DONE

*   [Ask whether the depot is starting to treat you like one of its own]
	# NPC_TRUST:DispatcherSafaa,3
	# STRESS:-1
	# MESSAGE:Safaa gives the closest thing to approval she believes in.
	She says the depot belongs to no one, but it does stop fighting you when you learn its rhythm.
	-> DONE

=== iman_laundry_lean ===
Iman notices the way you ask about prices before you ask about work. In the laundry, a lean week announces itself through small hesitations around ruined cloth and overdue payments.

*   [Ask which families are holding on by pretending nothing is wrong]
	# NPC_TRUST:LaundryOwnerIman,4
	# MESSAGE:Iman answers because she is too practical to waste a useful truth.
	She says the first sign is always what people stop repairing, long before what they stop eating.
	-> DONE

*   [Ask whether she ever lets customers pay late]
	# NPC_TRUST:LaundryOwnerIman,2
	# STRESS:1
	# MESSAGE:Iman gives you the arithmetic, not the sentiment.
	She says mercy is real, but steam, soap, and rent still expect cash at the end of the week.
	-> DONE