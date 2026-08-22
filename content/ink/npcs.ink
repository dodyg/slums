# Recurring NPC scenes - 100 conversations per NPC context for 100-day variety

=== landlord_default_1 ===
The landlord checks his ledger while glancing at the stairs.

*   [Promise payment soon]
	# STRESS:3
	# NPC_TRUST:LandlordHajjMahmoud,2
	-> DONE

=== landlord_default_2 ===
Hajj Mahmoud sighs and mentions other tenants who pay on time.

*   [Nod quietly]
	# STRESS:4
	-> DONE

=== landlord_default_3 ===
His prayer beads click steadily as he waits for your answer.

*   [Ask for a few more days]
	# STRESS:4
	# NPC_TRUST:LandlordHajjMahmoud,2
	-> DONE

=== landlord_default_4 ===
The stairwell feels narrow with him standing there.

*   [Explain your situation]
	# STRESS:5
	# NPC_TRUST:LandlordHajjMahmoud,1
	-> DONE

=== landlord_warm_1 ===
The landlord mentions his own struggles with building costs.

*   [Express sympathy]
	# STRESS:-1
	# NPC_TRUST:LandlordHajjMahmoud,5
	-> DONE

=== landlord_warm_2 ===
He sets aside his ledger and listens more carefully.

*   [Share your plans honestly]
	# STRESS:-3
	# NPC_TRUST:LandlordHajjMahmoud,6
	-> DONE

=== landlord_warm_3 ===
Hajj Mahmoud makes tea while discussing the rent.

*   [Accept his hospitality]
	# STRESS:-4
	# NPC_TRUST:LandlordHajjMahmoud,5
	-> DONE

=== landlord_warm_4 ===
Hajj Mahmoud offers a tired smile. Times are hard for everyone.

*   [Thank him for understanding]
	# STRESS:-2
	# NPC_TRUST:LandlordHajjMahmoud,4
	-> DONE

=== landlord_rent_negotiation ===
Hajj Mahmoud waits at the stairwell, prayer beads in hand. He asks for the rent without raising his voice, which somehow makes it worse.

*   [Answer politely and ask for time]
	# STRESS:5
	# NPC_TRUST:LandlordHajjMahmoud,8
	# MESSAGE:Hajj Mahmoud gives you a little room to breathe.
	You keep your voice low and tell him what little truth you can afford. He mutters that sabr has limits, but he steps aside and lets the matter rest for today.
	-> DONE

*   [Answer defiantly]
	# STRESS:10
	# NPC_TRUST:LandlordHajjMahmoud,-12
	# MESSAGE:The exchange hardens. The rent feels heavier already.
	You answer with the sharpness that comes from being cornered too often. He says nothing for a moment. Then he nods once, the way people do when they decide not to forget.
	-> DONE

=== landlord_rent_broke ===
Hajj Mahmoud studies your face before he looks at the ledger. Even he can tell this week has gone badly.

*   [Admit you are behind]
	# STRESS:4
	# NPC_TRUST:LandlordHajjMahmoud,3
	You tell him the truth in plain words. He does not forgive the debt, but he stops pressing for humiliation on top of it.
	-> DONE

=== fixer_first_contact ===
Umm Karim watches the crowd instead of you. She says there are always errands for women who keep their mouths shut and their feet moving.

*   [Listen carefully]
	# NPC_TRUST:FixerUmmKarim,8
	# FACTION_REP:ImbabaCrew,6
	# FLAG:fixer_met
	# MESSAGE:Umm Karim decides you may be useful.
	She gives you no names and no promises. Only times, corners, and the warning that loose talk gets people buried socially if not literally.
	-> DONE

*   [Refuse and leave]
	# STRESS:-2
	# NPC_TRUST:FixerUmmKarim,-6
	# MESSAGE:You walk away, but the offer lingers in your mind.
	Umm Karim shrugs as if she expected nothing more. In Cairo, another desperate person is never far away.
	-> DONE

=== fixer_double_life ===
Umm Karim smiles with half her mouth, as if even that is borrowed. In this part of Cairo, two stories belong to the same woman more often than one honest version ever could.

*   [Keep listening]
	# STRESS:2
	She tells you just enough to remind you that everybody here performs survival in layers.
	-> DONE

=== nurse_salma ===
Nurse Salma keeps writing as you speak, her pen moving faster than the clinic line outside the door.

*   [Ask about extra shifts]
	# NPC_TRUST:NurseSalma,4
	She finally looks up. "Extra shifts exist," she says, "but so do ten cousins and twenty debts. Tell me why I should risk my name for you."
	-> DONE

*   [Ask quietly about cheap medicine for your mother]
	# NPC_TRUST:NurseSalma,6
	# MESSAGE:Salma hears the fear under your voice before she answers.
	She lowers her voice. "There may be something after evening rounds. Don't ask for charity. Ask me what has not been recorded yet."
	-> DONE

=== nurse_salma_debt ===
Salma does not mention the medicine directly. That is how you know she remembers exactly what it cost her to help you last time.

*   [Acknowledge the debt]
	# DEBT:NurseSalma,true
	# NPC_TRUST:NurseSalma,2
	You tell her you have not forgotten. Her shoulders loosen by a fraction, which counts as grace in a clinic like this.
	-> DONE

=== hanan_fence ===
Hanan leans against the shuttered kiosk like she owns the hour between closing and trouble.

*   [Ask what kind of goods move quietly this week]
	# NPC_TRUST:FenceHanan,4
	She names categories instead of objects, neighborhoods instead of buyers, and leaves the dangerous parts for you to imagine.
	-> DONE

*   [Ask for easy money]
	# STRESS:3
	# NPC_TRUST:FenceHanan,-3
	Hanan laughs once. "Easy money is what people call it before they owe someone interest on their fear."
	-> DONE

=== neighbor_mona_heat ===
Mona from upstairs does not start with gossip this time. She starts by asking whether anyone has been following you home.

*   [Tell her no]
	# STRESS:2
	You lie automatically. She hears it anyway and decides not to shame you for it.
	-> DONE

=== mariam_pharmacy_urgent ===
Mariam hears the urgency before the details are finished. She reaches for the blister packs first and the questions second.

*   [Explain your mother's condition]
	# NPC_TRUST:PharmacistMariam,3
	She tells you what she can spare cheaply and what will still cost more than you can manage.
	-> DONE

=== safaa_depot_regular ===
Safaa barely glances up when you arrive. There is something bleakly reassuring about being expected.

*   [Take the ledger and start]
	# NPC_TRUST:DispatcherSafaa,2
	She nods once, as if punctuality is the only kind of intimacy work is allowed to have.
	-> DONE

=== landlord_hostile_1 ===
The landlord mentions he knows people who want the flat.

*   [Stay silent]
	# STRESS:10
	# NPC_TRUST:LandlordHajjMahmoud,-3
	-> DONE

=== landlord_hostile_2 ===
His voice is cold as he counts the days overdue.

*   [Promise immediate payment]
	# STRESS:9
	# NPC_TRUST:LandlordHajjMahmoud,1
	-> DONE

=== landlord_hostile_3 ===
He stands blocking your door with arms crossed.

*   [Ask what he needs]
	# STRESS:10
	# NPC_TRUST:LandlordHajjMahmoud,-2
	-> DONE

=== landlord_hostile_4 ===
Hajj Mahmoud's jaw is tight before you speak.

*   [Apologize for the delay]
	# STRESS:8
	# NPC_TRUST:LandlordHajjMahmoud,2
	-> DONE

=== landlord_broke_1 ===
The landlord's patience has limits but his voice stays low.

*   [Ask for a few days]
	# STRESS:5
	# NPC_TRUST:LandlordHajjMahmoud,4
	-> DONE

=== landlord_broke_2 ===
He studies your worn sleeves and empty hands.

*   [Explain honestly]
	# STRESS:6
	# NPC_TRUST:LandlordHajjMahmoud,3
	-> DONE

=== landlord_broke_3 ===
The ledger stays closed as he waits for your words.

*   [Tell him when you can pay]
	# STRESS:5
	# NPC_TRUST:LandlordHajjMahmoud,4
	-> DONE

=== landlord_broke_4 ===
Hajj Mahmoud sees the shortage in your face.

*   [Admit the difficulty]
	# STRESS:6
	# NPC_TRUST:LandlordHajjMahmoud,3
	-> DONE

=== landlord_broke_soft_1 ===
The landlord lowers his voice in the stairwell.

*   [Promise to catch up next week]
	# STRESS:3
	# NPC_TRUST:LandlordHajjMahmoud,6
	-> DONE

=== landlord_broke_soft_2 ===
He sees you trying and that matters to him.

*   [Thank him for the chance]
	# STRESS:-2
	# NPC_TRUST:LandlordHajjMahmoud,7
	-> DONE

=== landlord_broke_soft_3 ===
His hand rests on your shoulder briefly.

*   [Promise you will not let him down]
	# STRESS:-1
	# NPC_TRUST:LandlordHajjMahmoud,6
	-> DONE

=== landlord_broke_soft_4 ===
Hajj Mahmoud's disappointment is real but so is his tired mercy.

*   [Offer what you can pay now]
	# STRESS:4
	# NPC_TRUST:LandlordHajjMahmoud,5
	-> DONE

=== fixer_first_1 ===
She mentions errands for women who stay quiet.

*   [Ask what kind of work]
	# STRESS:2
	# NPC_TRUST:FixerUmmKarim,5
	-> DONE

=== fixer_first_2 ===
The market crowd moves around you both.

*   [Hear her terms]
	# STRESS:4
	# NPC_TRUST:FixerUmmKarim,3
	-> DONE

=== fixer_first_3 ===
Umm Karim tests your nerve with her gaze.

*   [Show you are willing to listen]
	# STRESS:3
	# NPC_TRUST:FixerUmmKarim,4
	-> DONE

=== fixer_first_4 ===
Umm Karim watches you with calculating eyes.

*   [Listen to her offer]
	# STRESS:3
	# NPC_TRUST:FixerUmmKarim,4
	-> DONE

=== fixer_repeat_1 ===
She has more tasks if you are ready.

*   [Accept the work]
	# STRESS:3
	# NPC_TRUST:FixerUmmKarim,4
	-> DONE

=== fixer_repeat_2 ===
Her network recognizes you now.

*   [Ask what she needs done]
	# STRESS:2
	# NPC_TRUST:FixerUmmKarim,4
	-> DONE

=== fixer_repeat_3 ===
Umm Karim nods when you approach.

*   [Wait for her instructions]
	# STRESS:1
	# NPC_TRUST:FixerUmmKarim,5
	-> DONE

=== fixer_repeat_4 ===
Umm Karim acknowledges you without surprise.

*   [Ask for available work]
	# STRESS:2
	# NPC_TRUST:FixerUmmKarim,3
	-> DONE

=== fixer_trusted_1 ===
She trusts you with better information.

*   [Listen carefully]
	# STRESS:1
	# NPC_TRUST:FixerUmmKarim,4
	-> DONE

=== fixer_trusted_2 ===
The network opens doors it kept closed before.

*   [Step through carefully]
	# STRESS:2
	# NPC_TRUST:FixerUmmKarim,4
	-> DONE

=== fixer_trusted_3 ===
Umm Karim values your reliability.

*   [Accept the responsibility]
	# STRESS:1
	# NPC_TRUST:FixerUmmKarim,5
	-> DONE

=== fixer_trusted_4 ===
Umm Karim speaks to you like an equal.

*   [Ask about serious work]
	# STRESS:2
	# NPC_TRUST:FixerUmmKarim,3
	-> DONE

=== fixer_double_life_1 ===
She seems amused by your caution.

*   [Ask if it affects your standing]
	# STRESS:2
	# NPC_TRUST:FixerUmmKarim,2
	-> DONE

=== fixer_double_life_2 ===
The double life is harder to hide from her.

*   [Admit the struggle]
	# STRESS:4
	# NPC_TRUST:FixerUmmKarim,3
	-> DONE

=== fixer_double_life_3 ===
Umm Karim has seen this balancing act before.

*   [Listen to her advice]
	# STRESS:2
	# NPC_TRUST:FixerUmmKarim,4
	-> DONE

=== fixer_double_life_4 ===
Umm Karim notices you juggling two lives.

*   [Explain the honest work helps cover]
	# STRESS:3
	# NPC_TRUST:FixerUmmKarim,3
	-> DONE

=== fixer_refusal_1 ===
She waits to see if your nerve has improved.

*   [Ask for another chance]
	# STRESS:5
	# NPC_TRUST:FixerUmmKarim,4
	-> DONE

=== fixer_refusal_2 ===
The door is not closed but it is narrower.

*   [Prove yourself this time]
	# STRESS:4
	# NPC_TRUST:FixerUmmKarim,3
	-> DONE

=== fixer_refusal_3 ===
Umm Karim gives you a second opportunity.

*   [Do not waste it]
	# STRESS:3
	# NPC_TRUST:FixerUmmKarim,4
	-> DONE

=== fixer_refusal_4 ===
Umm Karim remembers you walked away last time.

*   [Apologize for the hesitation]
	# STRESS:4
	# NPC_TRUST:FixerUmmKarim,3
	-> DONE

=== officer_default_1 ===
He waves you through without much interest.

*   [Thank him and move on]
	# STRESS:2
	-> DONE

=== officer_default_2 ===
The checkpoint is quiet this hour.

*   [Cooperate fully]
	# STRESS:3
	# NPC_TRUST:OfficerKhalid,3
	-> DONE

=== officer_default_3 ===
Khalid asks where you are headed.

*   [Give a simple answer]
	# STRESS:3
	# NPC_TRUST:OfficerKhalid,2
	-> DONE

=== officer_default_4 ===
Officer Khalid stops you with a routine check.

*   [Answer calmly]
	# STRESS:4
	# NPC_TRUST:OfficerKhalid,2
	-> DONE

=== officer_hot_1 ===
The streets are being watched more carefully.

*   [Nod and move along]
	# STRESS:5
	-> DONE

=== officer_hot_2 ===
His eyes stay on you longer than comfortable.

*   [Keep walking naturally]
	# STRESS:7
	# NPC_TRUST:OfficerKhalid,-1
	-> DONE

=== officer_hot_3 ===
The checkpoint has more officers than usual.

*   [Answer carefully]
	# STRESS:6
	# NPC_TRUST:OfficerKhalid,0
	-> DONE

=== officer_hot_4 ===
Khalid's questions are more pointed today.

*   [Stay calm and brief]
	# STRESS:6
	# NPC_TRUST:OfficerKhalid,1
	-> DONE

=== officer_marked_1 ===
He remembers your previous encounters.

*   [Act natural]
	# STRESS:7
	# NPC_TRUST:OfficerKhalid,-2
	-> DONE

=== officer_marked_2 ===
His gaze follows you through the crowd.

*   [Do not look back]
	# STRESS:9
	# NPC_TRUST:OfficerKhalid,-2
	-> DONE

=== officer_marked_3 ===
Officer Khalid makes a note in his book.

*   [Continue on your way]
	# STRESS:8
	# NPC_TRUST:OfficerKhalid,-1
	-> DONE

=== officer_marked_4 ===
Khalid recognizes your face with uncomfortable precision.

*   [Keep your answers short]
	# STRESS:8
	# NPC_TRUST:OfficerKhalid,-1
	-> DONE

=== mona_default_1 ===
She has heard interesting things today.

*   [Ask what people are saying]
	# STRESS:-1
	# NPC_TRUST:NeighborMona,5
	-> DONE

=== mona_default_2 ===
The stairwell carries voices and gossip.

*   [Share what you know]
	# STRESS:-2
	# NPC_TRUST:NeighborMona,4
	-> DONE

=== mona_default_3 ===
Mona's bowl of food is warm as she talks.

*   [Accept a taste]
	# STRESS:-3
	# NPC_TRUST:NeighborMona,5
	-> DONE

=== mona_default_4 ===
Mona catches you on the landing with building news.

*   [Listen to her updates]
	# STRESS:-2
	# NPC_TRUST:NeighborMona,4
	-> DONE

=== mona_warm_1 ===
She looks out for you more openly now.

*   [Thank her for her kindness]
	# STRESS:-3
	# NPC_TRUST:NeighborMona,6
	-> DONE

=== mona_warm_2 ===
The tea is sweet and the news is better.

*   [Enjoy the moment]
	# STRESS:-5
	# NPC_TRUST:NeighborMona,6
	-> DONE

=== mona_warm_3 ===
Mona treats you like family.

*   [Return the warmth]
	# STRESS:-4
	# NPC_TRUST:NeighborMona,7
	-> DONE

=== mona_warm_4 ===
Mona has tea ready and a warm smile.

*   [Accept her hospitality]
	# STRESS:-4
	# NPC_TRUST:NeighborMona,5
	-> DONE

=== mona_helped_1 ===
She trusts you more since that day.

*   [Acknowledge the bond]
	# STRESS:-3
	# NPC_TRUST:NeighborMona,4
	-> DONE

=== mona_helped_2 ===
The debt between you is understood.

*   [Offer help if she needs it]
	# STRESS:-2
	# NPC_TRUST:NeighborMona,5
	-> DONE

=== mona_helped_3 ===
Mona watches your back more carefully now.

*   [Thank her quietly]
	# STRESS:-3
	# NPC_TRUST:NeighborMona,4
	-> DONE

=== mona_helped_4 ===
Mona remembers when you stepped in for her.

*   [Downplay the help]
	# STRESS:-2
	# NPC_TRUST:NeighborMona,3
	-> DONE

=== mona_lean_1 ===
She knows where help might be found.

*   [Accept her guidance]
	# STRESS:-2
	# NPC_TRUST:NeighborMona,5
	-> DONE

=== mona_lean_2 ===
Her voice drops to share resources.

*   [Listen gratefully]
	# STRESS:-1
	# NPC_TRUST:NeighborMona,4
	-> DONE

=== mona_lean_3 ===
Mona knows which doors open for women in trouble.

*   [Follow her advice]
	# STRESS:-2
	# NPC_TRUST:NeighborMona,5
	-> DONE

=== mona_lean_4 ===
Mona notices your lean week without saying it directly.

*   [Let her speak]
	# STRESS:-1
	# NPC_TRUST:NeighborMona,4
	-> DONE

=== mona_heat_1 ===
She has heard whispers about police attention.

*   [Ask who is talking]
	# STRESS:1
	# NPC_TRUST:NeighborMona,4
	-> DONE

=== mona_heat_2 ===
Her eyes are worried as she pulls you close.

*   [Hear what she knows]
	# STRESS:1
	# NPC_TRUST:NeighborMona,5
	-> DONE

=== mona_heat_3 ===
Mona risks her safety to warn you.

*   [Thank her profoundly]
	# STRESS:0
	# NPC_TRUST:NeighborMona,6
	-> DONE

=== mona_heat_4 ===
Mona checks the stairwell before speaking quietly.

*   [Listen to her warning]
	# STRESS:-1
	# NPC_TRUST:NeighborMona,5
	-> DONE

=== salma_default_1 ===
She points you toward useful information.

*   [Thank her]
	# STRESS:1
	# NPC_TRUST:NurseSalma,3
	-> DONE

=== salma_default_2 ===
The clinic is busy but she makes time.

*   [Explain what you need]
	# STRESS:2
	# NPC_TRUST:NurseSalma,4
	-> DONE

=== salma_default_3 ===
Salma's hands never stop moving.

*   [Offer to help]
	# STRESS:1
	# NPC_TRUST:NurseSalma,5
	-> DONE

=== salma_default_4 ===
Nurse Salma moves quickly through her duties.

*   [Ask about available work]
	# STRESS:2
	# NPC_TRUST:NurseSalma,4
	-> DONE

=== salma_warm_1 ===
She trusts you with more responsibility.

*   [Accept the task]
	# STRESS:-2
	# NPC_TRUST:NurseSalma,6
	-> DONE

=== salma_warm_2 ===
The clinic feels less overwhelming with her guidance.

*   [Learn from her]
	# STRESS:-2
	# NPC_TRUST:NurseSalma,5
	-> DONE

=== salma_warm_3 ===
Salma treats you like part of the team.

*   [Step up]
	# STRESS:-1
	# NPC_TRUST:NurseSalma,6
	-> DONE

=== salma_warm_4 ===
Salma makes room for you in her busy schedule.

*   [Ask how you can help]
	# STRESS:-1
	# NPC_TRUST:NurseSalma,5
	-> DONE

=== salma_debt_1 ===
She doesn't push but the debt is understood.

*   [Offer work in exchange]
	# STRESS:2
	# NPC_TRUST:NurseSalma,4
	-> DONE

=== salma_debt_2 ===
The medicine she provided was not free.

*   [Acknowledge the debt]
	# STRESS:4
	# NPC_TRUST:NurseSalma,2
	-> DONE

=== salma_debt_3 ===
Salma's kindness has a ledger.

*   [Start paying it back]
	# STRESS:3
	# NPC_TRUST:NurseSalma,4
	-> DONE

=== salma_debt_4 ===
Salma remembers what she covered for your mother.

*   [Promise to repay]
	# STRESS:3
	# NPC_TRUST:NurseSalma,3
	-> DONE

=== salma_debt_warm_1 ===
She values your character over quick repayment.

*   [Promise to make it right]
	# STRESS:0
	# NPC_TRUST:NurseSalma,5
	-> DONE

=== salma_debt_warm_2 ===
The debt binds you closer rather than pushing apart.

*   [Return her trust]
	# STRESS:-1
	# NPC_TRUST:NurseSalma,5
	-> DONE

=== salma_debt_warm_3 ===
Salma sees you trying and that matters.

*   [Keep your word]
	# STRESS:0
	# NPC_TRUST:NurseSalma,6
	-> DONE

=== salma_debt_warm_4 ===
Salma's look is less creditor, more friend.

*   [Express gratitude]
	# STRESS:-1
	# NPC_TRUST:NurseSalma,4
	-> DONE

=== salma_urgent_1 ===
She shifts into crisis mode.

*   [Follow her instructions]
	# STRESS:3
	# NPC_TRUST:NurseSalma,6
	-> DONE

=== salma_urgent_2 ===
The clinic moves faster around your emergency.

*   [Trust her judgment]
	# STRESS:2
	# NPC_TRUST:NurseSalma,5
	-> DONE

=== salma_urgent_3 ===
Salma cuts through the waiting line.

*   [Stay close]
	# STRESS:3
	# NPC_TRUST:NurseSalma,6
	-> DONE

=== salma_urgent_4 ===
Salma sees the urgency in your face.

*   [Explain the situation]
	# STRESS:2
	# NPC_TRUST:NurseSalma,5
	-> DONE

=== salma_suspicious_1 ===
She seems to be piecing something together.

*   [Change the subject]
	# STRESS:5
	# NPC_TRUST:NurseSalma,-2
	-> DONE

=== salma_suspicious_2 ===
Her questions have an edge they lacked before.

*   [Deflect carefully]
	# STRESS:5
	# NPC_TRUST:NurseSalma,-1
	-> DONE

=== salma_suspicious_3 ===
The clinic feels less welcoming under her gaze.

*   [Keep the visit short]
	# STRESS:6
	# NPC_TRUST:NurseSalma,-2
	-> DONE

=== salma_suspicious_4 ===
Salma watches you with new eyes.

*   [Stay calm]
	# STRESS:4
	# NPC_TRUST:NurseSalma,1
	-> DONE

=== abu_samir_default_1 ===
He gestures toward an empty station.

*   [Get to work]
	# STRESS:2
	# NPC_TRUST:WorkshopBossAbuSamir,5
	-> DONE

=== abu_samir_default_2 ===
The workshop smells of hot fabric and steam.

*   [Find your place]
	# STRESS:2
	# NPC_TRUST:WorkshopBossAbuSamir,4
	-> DONE

=== abu_samir_default_3 ===
Abu Samir's rules are simple.

*   [Follow them]
	# STRESS:1
	# NPC_TRUST:WorkshopBossAbuSamir,5
	-> DONE

=== abu_samir_default_4 ===
Abu Samir counts finished pieces without looking up.

*   [Ask for a shift]
	# STRESS:3
	# NPC_TRUST:WorkshopBossAbuSamir,4
	-> DONE

=== abu_samir_warm_1 ===
He has a better table for you today.

*   [Thank him]
	# STRESS:-2
	# NPC_TRUST:WorkshopBossAbuSamir,6
	-> DONE

=== abu_samir_warm_2 ===
The workshop feels more like your place now.

*   [Settle in]
	# STRESS:-2
	# NPC_TRUST:WorkshopBossAbuSamir,5
	-> DONE

=== abu_samir_warm_3 ===
Abu Samir trusts your hands with the good cloth.

*   [Do not let him down]
	# STRESS:-1
	# NPC_TRUST:WorkshopBossAbuSamir,6
	-> DONE

=== abu_samir_warm_4 ===
Abu Samir nods when you enter.

*   [Ask about steady work]
	# STRESS:-1
	# NPC_TRUST:WorkshopBossAbuSamir,5
	-> DONE

=== abu_samir_cold_1 ===
He points to a less desirable station.

*   [Accept without complaint]
	# STRESS:5
	# NPC_TRUST:WorkshopBossAbuSamir,2
	-> DONE

=== abu_samir_cold_2 ===
The workshop feels unwelcoming.

*   [Keep your head down]
	# STRESS:5
	# NPC_TRUST:WorkshopBossAbuSamir,0
	-> DONE

=== abu_samir_cold_3 ===
Abu Samir's trust is hard to win back.

*   [Work harder]
	# STRESS:4
	# NPC_TRUST:WorkshopBossAbuSamir,1
	-> DONE

=== abu_samir_cold_4 ===
Abu Samir doesn't acknowledge your presence immediately.

*   [Wait patiently]
	# STRESS:4
	# NPC_TRUST:WorkshopBossAbuSamir,1
	-> DONE

=== abu_samir_embarrassed_1 ===
He gives you a chance to prove yourself.

*   [Promise better work]
	# STRESS:2
	# NPC_TRUST:WorkshopBossAbuSamir,3
	-> DONE

=== abu_samir_embarrassed_2 ===
The memory of the error hangs between you.

*   [Own it completely]
	# STRESS:3
	# NPC_TRUST:WorkshopBossAbuSamir,3
	-> DONE

=== abu_samir_embarrassed_3 ===
Abu Samir's patience is tested.

*   [Rebuild his trust]
	# STRESS:2
	# NPC_TRUST:WorkshopBossAbuSamir,4
	-> DONE

=== abu_samir_embarrassed_4 ===
Abu Samir remembers your last mistake.

*   [Apologize directly]
	# STRESS:3
	# NPC_TRUST:WorkshopBossAbuSamir,2
	-> DONE

=== nadia_default_1 ===
She points to the evening rush.

*   [Agree to help]
	# STRESS:3
	# NPC_TRUST:CafeOwnerNadia,5
	-> DONE

=== nadia_default_2 ===
The cafe hums with conversation and steam.

*   [Find your place]
	# STRESS:2
	# NPC_TRUST:CafeOwnerNadia,4
	-> DONE

=== nadia_default_3 ===
Nadia reads customers and workers alike.

*   [Follow her lead]
	# STRESS:1
	# NPC_TRUST:CafeOwnerNadia,5
	-> DONE

=== nadia_default_4 ===
Nadia runs her cafe with sharp eyes.

*   [Ask about work]
	# STRESS:2
	# NPC_TRUST:CafeOwnerNadia,4
	-> DONE

=== nadia_warm_1 ===
She mentions better shifts coming up.

*   [Express interest]
	# STRESS:-1
	# NPC_TRUST:CafeOwnerNadia,6
	-> DONE

=== nadia_warm_2 ===
The cafe feels more welcoming now.

*   [Settle in]
	# STRESS:-3
	# NPC_TRUST:CafeOwnerNadia,6
	-> DONE

=== nadia_warm_3 ===
Nadia trusts you with the good customers.

*   [Do not disappoint]
	# STRESS:-2
	# NPC_TRUST:CafeOwnerNadia,7
	-> DONE

=== nadia_warm_4 ===
Nadia slides tea toward you without asking.

*   [Thank her]
	# STRESS:-2
	# NPC_TRUST:CafeOwnerNadia,5
	-> DONE

=== nadia_cold_1 ===
She doesn't offer extra work today.

*   [Accept and leave]
	# STRESS:4
	# NPC_TRUST:CafeOwnerNadia,0
	-> DONE

=== nadia_cold_2 ===
The cafe feels less open to you.

*   [Order and go]
	# STRESS:4
	# NPC_TRUST:CafeOwnerNadia,-1
	-> DONE

=== nadia_cold_3 ===
Nadia's warmth has cooled.

*   [Give her space]
	# STRESS:3
	# NPC_TRUST:CafeOwnerNadia,0
	-> DONE

=== nadia_cold_4 ===
Nadia is civil but distant.

*   [Keep the conversation brief]
	# STRESS:3
	# NPC_TRUST:CafeOwnerNadia,1
	-> DONE

=== nadia_double_life_1 ===
She seems to understand more than she says.

*   [Change the subject]
	# STRESS:2
	# NPC_TRUST:CafeOwnerNadia,2
	-> DONE

=== nadia_double_life_2 ===
The cafe owner notices the shadows under your eyes.

*   [Offer a simple explanation]
	# STRESS:3
	# NPC_TRUST:CafeOwnerNadia,1
	-> DONE

=== nadia_double_life_3 ===
Nadia's knowing look is uncomfortable.

*   [Accept her discretion]
	# STRESS:2
	# NPC_TRUST:CafeOwnerNadia,3
	-> DONE

=== nadia_double_life_4 ===
Nadia studies your tired expression.

*   [Deflect the observation]
	# STRESS:3
	# NPC_TRUST:CafeOwnerNadia,1
	-> DONE

=== hanan_default_1 ===
She has information if you are careful.

*   [Listen to her terms]
	# STRESS:2
	# NPC_TRUST:FenceHanan,5
	-> DONE

=== hanan_default_2 ===
The market noise covers quiet conversation.

*   [Lean in closer]
	# STRESS:2
	# NPC_TRUST:FenceHanan,4
	-> DONE

=== hanan_default_3 ===
Hanan knows what moves and what stays still.

*   [Ask carefully]
	# STRESS:3
	# NPC_TRUST:FenceHanan,5
	-> DONE

=== hanan_default_4 ===
Hanan watches the market crowd from her spot.

*   [Ask about quiet goods]
	# STRESS:3
	# NPC_TRUST:FenceHanan,4
	-> DONE

=== hanan_warm_1 ===
She shares market intelligence freely.

*   [Thank her]
	# STRESS:-2
	# NPC_TRUST:FenceHanan,6
	-> DONE

=== hanan_warm_2 ===
The trust is valuable in her world.

*   [Protect it]
	# STRESS:-1
	# NPC_TRUST:FenceHanan,6
	-> DONE

=== hanan_warm_3 ===
Hanan values your reliability.

*   [Maintain it]
	# STRESS:-2
	# NPC_TRUST:FenceHanan,7
	-> DONE

=== hanan_warm_4 ===
Hanan makes room for you in the shade.

*   [Ask what she has heard]
	# STRESS:-1
	# NPC_TRUST:FenceHanan,5
	-> DONE

=== hanan_cold_1 ===
She has nothing to offer today.

*   [Accept and move on]
	# STRESS:5
	# NPC_TRUST:FenceHanan,0
	-> DONE

=== hanan_cold_2 ===
The shade she offers others is closed to you.

*   [Walk away]
	# STRESS:5
	# NPC_TRUST:FenceHanan,-1
	-> DONE

=== hanan_cold_3 ===
Hanan's trust is hard to rebuild.

*   [Try again later]
	# STRESS:4
	# NPC_TRUST:FenceHanan,1
	-> DONE

=== hanan_cold_4 ===
Hanan barely acknowledges you.

*   [Keep the interaction short]
	# STRESS:4
	# NPC_TRUST:FenceHanan,1
	-> DONE

=== youssef_default_1 ===
He shares useful observations.

*   [Pay attention]
	# STRESS:1
	# NPC_TRUST:RunnerYoussef,5
	-> DONE

=== youssef_default_2 ===
The square is his territory.

*   [Follow his lead]
	# STRESS:2
	# NPC_TRUST:RunnerYoussef,4
	-> DONE

=== youssef_default_3 ===
Youssef knows which way the wind blows.

*   [Listen to his forecast]
	# STRESS:1
	# NPC_TRUST:RunnerYoussef,5
	-> DONE

=== youssef_default_4 ===
Youssef drifts through the square with restless energy.

*   [Ask about police movements]
	# STRESS:2
	# NPC_TRUST:RunnerYoussef,4
	-> DONE

=== youssef_hot_1 ===
His voice stays low and cautious.

*   [Listen carefully]
	# STRESS:4
	# NPC_TRUST:RunnerYoussef,4
	-> DONE

=== youssef_hot_2 ===
The square feels watched today.

*   [Stay alert]
	# STRESS:4
	# NPC_TRUST:RunnerYoussef,3
	-> DONE

=== youssef_hot_3 ===
Youssef's usual ease is replaced by tension.

*   [Match his energy]
	# STRESS:3
	# NPC_TRUST:RunnerYoussef,4
	-> DONE

=== youssef_hot_4 ===
Youssef checks the street before speaking.

*   [Ask what he has heard]
	# STRESS:3
	# NPC_TRUST:RunnerYoussef,3
	-> DONE

=== youssef_embedded_1 ===
He trusts you with better routes.

*   [Memorize them]
	# STRESS:0
	# NPC_TRUST:RunnerYoussef,5
	-> DONE

=== youssef_embedded_2 ===
The network recognizes your place in it.

*   [Accept the trust]
	# STRESS:1
	# NPC_TRUST:RunnerYoussef,5
	-> DONE

=== youssef_embedded_3 ===
Youssef shares secrets meant for few.

*   [Guard them]
	# STRESS:0
	# NPC_TRUST:RunnerYoussef,6
	-> DONE

=== youssef_embedded_4 ===
Youssef talks to you like an insider.

*   [Share what you know]
	# STRESS:1
	# NPC_TRUST:RunnerYoussef,4
	-> DONE

=== mariam_default_1 ===
She has work if you want it.

*   [Accept the offer]
	# STRESS:1
	# NPC_TRUST:PharmacistMariam,5
	-> DONE

=== mariam_default_2 ===
The pharmacy is busy but organized.

*   [Find your place]
	# STRESS:2
	# NPC_TRUST:PharmacistMariam,4
	-> DONE

=== mariam_default_3 ===
Mariam's efficiency is impressive.

*   [Learn from it]
	# STRESS:1
	# NPC_TRUST:PharmacistMariam,5
	-> DONE

=== mariam_default_4 ===
Mariam labels shelves faster than the queue empties.

*   [Ask about shifts]
	# STRESS:2
	# NPC_TRUST:PharmacistMariam,4
	-> DONE

=== mariam_warm_1 ===
She mentions you when good work comes up.

*   [Thank her]
	# STRESS:-2
	# NPC_TRUST:PharmacistMariam,6
	-> DONE

=== mariam_warm_2 ===
The pharmacy feels more like your place.

*   [Settle in]
	# STRESS:-2
	# NPC_TRUST:PharmacistMariam,6
	-> DONE

=== mariam_warm_3 ===
Mariam trusts your judgment with customers.

*   [Do not let her down]
	# STRESS:-1
	# NPC_TRUST:PharmacistMariam,7
	-> DONE

=== mariam_warm_4 ===
Mariam makes space for you at the counter.

*   [Ask about steady hours]
	# STRESS:-1
	# NPC_TRUST:PharmacistMariam,5
	-> DONE

=== mariam_urgent_1 ===
She prioritizes your needs.

*   [Follow her advice]
	# STRESS:1
	# NPC_TRUST:PharmacistMariam,6
	-> DONE

=== mariam_urgent_2 ===
The pharmacy becomes a triage unit for your crisis.

*   [Accept the help]
	# STRESS:2
	# NPC_TRUST:PharmacistMariam,6
	-> DONE

=== mariam_urgent_3 ===
Mariam cuts through bureaucracy.

*   [Be grateful]
	# STRESS:1
	# NPC_TRUST:PharmacistMariam,7
	-> DONE

=== mariam_urgent_4 ===
Mariam hears urgency before you explain.

*   [Describe the situation]
	# STRESS:2
	# NPC_TRUST:PharmacistMariam,5
	-> DONE

=== safaa_default_1 ===
She has routes if you are ready.

*   [Accept]
	# STRESS:1
	# NPC_TRUST:DispatcherSafaa,5
	-> DONE

=== safaa_default_2 ===
The depot hums with engines and voices.

*   [Find your place]
	# STRESS:2
	# NPC_TRUST:DispatcherSafaa,4
	-> DONE

=== safaa_default_3 ===
Safaa reads the yard like a map.

*   [Learn from her]
	# STRESS:1
	# NPC_TRUST:DispatcherSafaa,5
	-> DONE

=== safaa_default_4 ===
Safaa manages the depot noise like a tool.

*   [Ask about work]
	# STRESS:2
	# NPC_TRUST:DispatcherSafaa,4
	-> DONE

=== safaa_warm_1 ===
She has something good for you.

*   [Accept with thanks]
	# STRESS:-2
	# NPC_TRUST:DispatcherSafaa,6
	-> DONE

=== safaa_warm_2 ===
The depot recognizes you now.

*   [Claim your place]
	# STRESS:-2
	# NPC_TRUST:DispatcherSafaa,6
	-> DONE

=== safaa_warm_3 ===
Safaa's trust means better routes.

*   [Do not waste it]
	# STRESS:-1
	# NPC_TRUST:DispatcherSafaa,7
	-> DONE

=== safaa_warm_4 ===
Safaa waves you closer without looking.

*   [Ask about better lines]
	# STRESS:-1
	# NPC_TRUST:DispatcherSafaa,5
	-> DONE

=== safaa_regular_1 ===
She treats you like part of the depot.

*   [Get to work]
	# STRESS:-1
	# NPC_TRUST:DispatcherSafaa,5
	-> DONE

=== safaa_regular_2 ===
The rhythm of the yard is familiar.

*   [Fall into step]
	# STRESS:-1
	# NPC_TRUST:DispatcherSafaa,5
	-> DONE

=== safaa_regular_3 ===
Safaa counts on you now.

*   [Show up consistently]
	# STRESS:0
	# NPC_TRUST:DispatcherSafaa,6
	-> DONE

=== safaa_regular_4 ===
Safaa barely looks surprised to see you.

*   [Ask what needs doing]
	# STRESS:0
	# NPC_TRUST:DispatcherSafaa,4
	-> DONE

=== iman_default_1 ===
She points to an empty station.

*   [Start working]
	# STRESS:1
	# NPC_TRUST:LaundryOwnerIman,5
	-> DONE

=== iman_default_2 ===
The laundry is hot and busy.

*   [Find your rhythm]
	# STRESS:2
	# NPC_TRUST:LaundryOwnerIman,4
	-> DONE

=== iman_default_3 ===
Iman's expectations are clear.

*   [Meet them]
	# STRESS:1
	# NPC_TRUST:LaundryOwnerIman,5
	-> DONE

=== iman_default_4 ===
Iman runs the laundry with steam and precision.

*   [Ask about pressing work]
	# STRESS:2
	# NPC_TRUST:LaundryOwnerIman,4
	-> DONE

=== iman_warm_1 ===
She trusts you with better tasks.

*   [Accept]
	# STRESS:-1
	# NPC_TRUST:LaundryOwnerIman,6
	-> DONE

=== iman_warm_2 ===
The laundry feels less like work and more like belonging.

*   [Settle in]
	# STRESS:-2
	# NPC_TRUST:LaundryOwnerIman,6
	-> DONE

=== iman_warm_3 ===
Iman's kindness makes the heat bearable.

*   [Return it]
	# STRESS:-1
	# NPC_TRUST:LaundryOwnerIman,7
	-> DONE

=== iman_warm_4 ===
Iman hands you water before you ask.

*   [Thank her]
	# STRESS:-2
	# NPC_TRUST:LaundryOwnerIman,5
	-> DONE

=== iman_lean_1 ===
She knows what families are cutting.

*   [Listen to her observations]
	# STRESS:1
	# NPC_TRUST:LaundryOwnerIman,5
	-> DONE

=== iman_lean_2 ===
The laundry owner sees your tight margins.

*   [Accept her understanding]
	# STRESS:1
	# NPC_TRUST:LaundryOwnerIman,4
	-> DONE

=== iman_lean_3 ===
Iman offers discreet help.

*   [Take it]
	# STRESS:0
	# NPC_TRUST:LaundryOwnerIman,5
	-> DONE

=== iman_lean_4 ===
Iman notices your hesitation around prices.

*   [Admit the difficulty]
	# STRESS:2
	# NPC_TRUST:LaundryOwnerIman,4
	-> DONE

=== tarek_default_1 ===
Tarek adjusts his cardboard display, glancing up as you pass. "You look like you need something. Or maybe you just need to stop walking for a minute."
*   [Browse his goods]
	# STRESS:1
	# NPC_TRUST:VendorTarek,2
	-> DONE

=== tarek_default_2 ===
The afternoon sun beats down on Tarek's makeshift stall. He fans himself with a flattened cardboard box. "This heat will cook us before it cooks the merchandise."
*   [Agree about the heat]
	# STRESS:2
	# NPC_TRUST:VendorTarek,1
	-> DONE

=== tarek_default_3 ===
Tarek counts small bills and stuffs them into his shirt pocket. "Twenty pounds profit today. Twenty. I used to make that in an hour."
*   [Ask what changed]
	# STRESS:3
	# NPC_TRUST:VendorTarek,2
	-> DONE

=== tarek_default_4 ===
A customer walks past without stopping. Tarek watches them go. "See that? That's my whole business plan. People walking past."
*   [Sympathize with the struggle]
	# STRESS:2
	# NPC_TRUST:VendorTarek,2
	-> DONE

