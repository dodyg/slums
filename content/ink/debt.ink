# Debt and loan shark escalation scenes

=== event_loan_shark_first_warning ===
The message arrives folded inside a cigarette packet, left at the ahwa where anyone could have placed it but nobody would have touched. The handwriting is deliberate, unhurried, and makes no threats. It only states what you owe, what you have not paid, and a date three days from now.

The date is underlined twice.

*   [Scrape together what you can]
    You count every coin twice, borrow a half-hour of worry from Mona, and consider whether asking the landlord for an advance would be worse than what the note promises. The answer depends on how seriously you take the underlines.
    # STRESS:6
    # MESSAGE:The loan shark's first warning arrives with a deadline you cannot ignore.
    -> DONE

*   [Ask Hanan for advice]
    Hanan reads the note once and folds it back the way it came. She says the handwriting is from someone who collects for a living, which means the next message will not arrive on paper.
    # STRESS:4
    # NPC_TRUST:FenceHanan,2
    # MESSAGE:Hanan warns you that the next visit will not be so polite.
    -> DONE

*   [Ignore it]
    You put the packet in the bin and tell yourself that urgency is just another kind of manipulation. Three days is a long time in Cairo. Something might change. Something always changes.
    # STRESS:8
    # MESSAGE:You ignore the warning. The deadline ticks louder in the back of your mind.
    -> DONE

=== event_loan_shark_visit ===
He does not knock. He leans against the doorframe and speaks through the gap as if the flat were already his to judge. Your mother is in the back room. He does not raise his voice, which is somehow worse than shouting. He explains what you owe, what the interest has become, and what the next conversation will cost if this one ends without payment.

Your mother coughs in the other room. His eyes flick toward the sound.

*   [Pay what you can and beg for time]
    You press crumpled notes into his hand and keep your eyes down. He counts without looking, nods once, and says the remaining amount just grew. But he leaves, and the door closes, and your mother does not ask who that was because she already knows the shape of debt even when nobody names it.
    # MONEY:-40
    # STRESS:8
    # HEALTH:-3
    # MESSAGE:You pay what you can. The loan shark leaves, but the debt swells.
    -> DONE

*   [Tell him you need one more week]
    He tilts his head as if considering a reasonable request from a reasonable person. Then he mentions your mother's medication schedule by name. He says one week. He does not say what happens after.
    # STRESS:12
    # HEALTH:-5
    # MESSAGE:The loan shark grants one more week. His knowledge of your mother's routine is a threat disguised as conversation.
    -> DONE

*   [Threaten to go to the police]
    The laugh is short and genuine, the way people laugh at children who threaten to tell the teacher. He says the police already know his name and his face and his route, and they have not stopped him yet. He suggests you think about who protects whom in this neighborhood before you pick up a phone.
    # STRESS:15
    # HEALTH:-8
    # MESSAGE:Your threat backfires. The loan shark knows his way around the system better than you do.
    -> DONE

=== event_loan_shark_ultimatum ===
Two men this time. One stays by the stairwell, watching the neighbors find urgent reasons to be elsewhere. The other sits in your single chair as if measuring the room for later.

He gives you forty-eight hours. He does not say what happens after. He does not have to. The building has already gone quiet, and when he leaves, the silence he takes with him is heavier than the silence he brought.

*   [Swallow your pride and beg everyone you know]
    You spend the next hour apologizing to people who have already given you enough, borrowing from trust you have not earned, and learning how many times {gender == "male": a man | a woman} can say "please" before the word loses all meaning.
    # STRESS:12
    # MESSAGE:You beg for help across every contact you have. The debt shrinks, but so does something harder to name.
    -> DONE

*   [Plan to leave Cairo by morning]
    You start throwing things into a bag before you realize there is nowhere to go that the debt has not already reached. Cairo is large enough to hide in, but not large enough to outrun what you owe.
    # STRESS:10
    # MESSAGE:Flight crosses your mind, but Cairo traps people as often as it shelters them.
    -> DONE

*   [Accept what comes]
    You sit on the mattress and wait. Sometimes the only choice left is the shape of your own stillness while the city decides what it will take from you next.
    # STRESS:15
    # MESSAGE:You wait. The deadline approaches like weather you cannot dress for.
    -> DONE

=== event_npc_loan_request_mona ===
Mona stands in your doorway twisting the hem of her gallabiya. She never twists fabric. She says something about her brother in Bulaq, about a medical bill, about how she has already asked everyone else and cannot ask them twice.

The amount is small by some standards. By hers, it is everything.

*   [Give her what she needs]
    You count out the notes and press them into her hand without ceremony. She tries to thank you and you tell her to save the gratitude for someone who can afford it. She laughs, but her eyes are wet.
    # MONEY:-25
    # NPC_TRUST:NeighborMona,5
    # FAVOR:NeighborMona
    # MESSAGE:Mona owes you now, and she is the kind of woman who remembers.
    -> DONE

*   [Offer what you can afford]
    You give her half. She takes it without counting and says it is more than anyone else managed. The gap between what she needs and what you can spare is the exact width of the entire neighborhood's poverty.
    # MONEY:-12
    # NPC_TRUST:NeighborMona,3
    # MESSAGE:You give Mona what you can. It helps, but not enough.
    -> DONE

*   [Apologize and refuse]
    You explain that your own walls are cracking, that the money is already spoken for by rent and medicine and the arithmetic of staying alive. She nods too quickly and leaves too quietly, and you know that refusing a woman like Mona costs more than money ever could.
    # NPC_TRUST:NeighborMona,-4
    # REFUSAL:NeighborMona
    # MESSAGE:Mona understands, but understanding is not the same as forgiving.
    -> DONE

=== event_npc_loan_request_youssef ===
Youssef does not ask directly. He mentions a debt of his own, a runner who got caught, a family that needs bail money he does not have. The request is wrapped in three layers of indirection because in his world asking plainly is a vulnerability.

*   [Lend him the money]
    You hand over the cash and he pockets it without looking. He says the streets remember who pays and who talks. In his vocabulary, that passes for gratitude.
    # MONEY:-30
    # NPC_TRUST:RunnerYoussef,5
    # FAVOR:RunnerYoussef
    # MESSAGE:Youssef owes you. In his world, that is a currency worth more than cash.
    -> DONE

*   [Offer a smaller amount]
    He takes it and does not complain, but his eyes do the math faster than his mouth can hide the shortfall. He says it is enough for now, and the "for now" hangs in the air like smoke.
    # MONEY:-15
    # NPC_TRUST:RunnerYoussef,2
    # MESSAGE:You help Youssef partially. The debt in his eyes is only partly paid.
    -> DONE

*   [Say you cannot help]
    He shrugs as if he expected nothing, which is how he protects himself from the disappointment of having asked at all. He leaves without another word, and you hear his footsteps skip the broken stair on the way down.
    # NPC_TRUST:RunnerYoussef,-3
    # REFUSAL:RunnerYoussef
    # MESSAGE:Youssef files the refusal somewhere behind his eyes and keeps moving.
    -> DONE

=== event_npc_loan_repay_mona ===
Mona appears at your door with a plate of stuffed vine leaves and an envelope. The vine leaves are fresh. The envelope contains slightly more than she borrowed.

She says the extra is not interest. It is just what neighbors do when the math of survival lets them breathe for one week.

*   [Accept gratefully]
    You take the plate and the envelope and tell her she did not have to. She says she knows, which is how the best generosity works in Cairo — acknowledged, then absorbed, then paid forward when the next person's walls crack.
    # MONEY:30
    # NPC_TRUST:NeighborMona,2
    # STRESS:-3
    # MESSAGE:Mona repays her debt with interest she calls neighborliness.
    -> DONE

=== event_npc_loan_repay_youssef ===
Youssef slides the money under your door in an envelope with no name on it. Later, in the lane, he mentions that a certain checkpoint will be empty tomorrow night, as if the information were unrelated to the cash.

*   [Thank him for both]
    You nod at the envelope and at the information, and he nods back, and the transaction completes itself in the way that Cairo's underground economy always does — through implication, timing, and the understanding that some currencies do not need counting.
    # MONEY:35
    # NPC_TRUST:RunnerYoussef,2
    # MESSAGE:Youssef repays in cash and in a tip about tomorrow night's checkpoint gap.
    -> DONE

=== event_npc_hardship_mona ===
Mona's usual energy dims over three days. She stops appearing in the stairwell with gossip. Her door stays shut. When you finally see her, she has lost weight she could not afford and is wearing the same gallabiya she wore yesterday.

She says her brother lost his job. She says it without drama because the drama has already happened inside the kitchen where nobody could see it.

*   [Bring her food from your own stock]
    You fill a bag with rice, lentils, and the last of the onions and leave it at her door. She opens it before you reach the stairs and says nothing, which is louder than any gratitude.
    # FOOD:-2
    # NPC_TRUST:NeighborMona,4
    # STRESS:-1
    # MESSAGE:You share what little you have. Mona's silence says more than any thanks.
    -> DONE

*   [Offer to help her find work]
    You sit with her and write down every contact, every employer, every rumor of an opening. By the end, the list is short enough to make you both quiet, but the act of writing it down is itself a kind of hope.
    # NPC_TRUST:NeighborMona,3
    # MESSAGE:You help Mona strategize. The list is short, but she is no longer alone with it.
    -> DONE

*   [Listen and let her grieve]
    Sometimes help means sitting in a kitchen that smells of old cooking gas and listening to a woman describe how fast everything can fall apart. You do not offer solutions. You offer presence, which in Cairo is the rarer gift.
    # NPC_TRUST:NeighborMona,2
    # STRESS:2
    # MESSAGE:You sit with Mona in her grief. It costs you nothing and everything.
    -> DONE

=== event_npc_windfall_nadia ===
Nadia is in a good mood, which is itself a kind of weather event. She says the ahwa had its best week in months, that a tour group found the alley by accident and stayed for two hours, that even the cat looked well-fed.

She slides a bonus across the counter and tells you not to get used to it.

*   [Thank her sincerely]
    You pocket the extra and tell her the tea has been better lately, which is true and also the kind of flattery that costs nothing in a city where everyone is always selling something.
    # MONEY:15
    # NPC_TRUST:CafeOwnerNadia,2
    # MESSAGE:Nadia shares a rare good week. The bonus is small but real.
    -> DONE

*   [Ask if she needs help with anything extra]
    She raises an eyebrow at the offer, then mentions that the storage room needs sorting and the evening shift could use one more pair of hands. The money is not much, but the hours are flexible and the tips are honest.
    # MONEY:10
    # NPC_TRUST:CafeOwnerNadia,3
    # MESSAGE:Nadia offers a few extra shifts. The money is modest but the gesture is genuine.
    -> DONE

=== event_debt_gossip ===
At the ahwa, someone mentions that Hanan has been collecting debts for a man in Dokki who does not use banks, courts, or patience. The information arrives wrapped in a joke about people who borrow money and then act surprised when memory comes knocking.

You laugh at the joke. You do not mention the note in your pocket.

*   [Ask around about this lender quietly]
    You buy two teas you cannot afford and ask questions small enough to fit inside casual conversation. What you learn is that the lender is patient right up until he is not, and the line between those two states is invisible until you have already crossed it.
    # MONEY:-6
    # STRESS:3
    # MESSAGE:You gather intelligence on the loan shark. The picture it paints is not reassuring.
    -> DONE

*   [Keep your head down and say nothing]
    You drink your tea and nod and file the information away with all the other things you know but cannot act on. In Cairo, knowing is not the same as being able to protect yourself.
    # STRESS:4
    # MESSAGE:You absorb the gossip silently. The debt presses harder against the walls of your chest.
    -> DONE

=== event_rumor_warning ===
Mona catches you in the stairwell and speaks without preamble. She says people are talking about the money, about where it comes from, about the company you have been keeping. She does not judge. She warns.

"Talk is cheap in Cairo until it arrives at the wrong ears. Then it costs everything."

*   [Thank her and ask who is talking]
    She names two neighbours and a shopkeeper who watches the alley from his window like it owes him money. You memorize the names and adjust your route home accordingly.
    # NPC_TRUST:NeighborMona,3
    # STRESS:3
    # MESSAGE:Mona warns you about the rumors. You adjust your routes.
    -> DONE

*   [Dismiss it as idle gossip]
    Mona's face tightens. She says that in this building, idle gossip has gotten people visited, questioned, and evicted in the same week. She does not push further. She has said her piece, and in Cairo, a warning delivered once is all that friendship owes.
    # NPC_TRUST:NeighborMona,-2
    # STRESS:5
    # MESSAGE:You brush off Mona's warning. Her face says she will not repeat it.
    -> DONE

=== event_community_debt_circle ===
After Friday prayer, three of the women from the building gather on the rooftop with tea and a notebook. They are pooling money: ten pounds here, twenty there, a promise of fifteen next week. The system is informal, handwritten, and older than any bank.

Umm Karim runs the numbers and tells you that your name has been added to the waiting list for the next cycle.

*   [Join the pool]
    You contribute what you can and write your name next to women who have been doing this for years. The returns are small, but they are honest, and the group watches its own with the ferocity of people who know exactly how thin the margin is between surviving and not.
    # MONEY:-15
    # STRESS:-4
    # FLAG:community_pool_joined
    # MESSAGE:You join the rooftop savings circle. The returns will come in time.
    -> DONE

*   [Decline for now]
    You thank them and say you will join when things stabilize. They nod without judgment, because every woman at that table has said the same thing at least once before the math finally allowed her to mean it.
    # STRESS:2
    # MESSAGE:You step back from the savings circle. The invitation remains open.
    -> DONE
