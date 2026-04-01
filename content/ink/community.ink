# Community solidarity and territory events

=== event_friday_rooftop ===
The rooftop on Friday afternoon belongs to everyone. Women spread mats across the concrete, children chase each other between the water tanks, and the call to prayer drifts up from the minaret two blocks away like a blanket being shaken out over the whole neighborhood.

Hajj Mahmoud sits on a plastic chair near the edge, prayer beads in hand, watching his tenants the way a shepherd watches sheep — with a mixture of duty and exhaustion that never quite becomes affection.

*   [Join the gathering]
    You sit cross-legged on a mat between Mona and a woman from the fourth floor whose name you should know by now. Tea is poured. News is exchanged. A child climbs into your lap uninvited and refuses to leave. For two hours, the rent is not discussed, the money is not counted, and the city feels like it belongs to the people sitting on its roof.
    # STRESS:-5
    # NPC_TRUST:NeighborMona,2
    # NPC_TRUST:LandlordHajjMahmoud,1
    # MESSAGE:The Friday rooftop gathering reminds you that the building holds more than walls.
    -> DONE

*   [Help with the tea and food distribution]
    You spend the afternoon carrying glasses, refilling the teapot, and making sure the older women get the cushions. It is the kind of work that is invisible until it stops happening, and everyone notices that you are the one doing it.
    # STRESS:-3
    # NPC_TRUST:NeighborMona,3
    # NPC_TRUST:FixerUmmKarim,1
    # MESSAGE:You serve the Friday gathering. The work is small, but the building sees it.
    -> DONE

=== event_neighborhood_cleanup ===
Somebody's cousin organized it. Somebody else brought bags. By nine in the morning, half the building is sweeping the stairwell, scrubbing the landing, and carrying trash to the skip three blocks away because the city trucks have not come in two weeks.

The work is not glamorous. The stairwell smells like ammonia and old cooking oil. But by noon, the building looks almost proud of itself, and people who never speak to each other are passing cleaning supplies and complaints about the municipal government with equal ease.

*   [Join the cleanup crew]
    You scrub the stairs with a brush that has seen better decades and carry bags that test your lower back. By the end, your hands are raw, your clothes are ruined, and the building looks marginally less like it is decomposing. The neighbors thank you with the sincerity of people who genuinely needed the help.
    # ENERGY:-8
    # STRESS:-4
    # NPC_TRUST:NeighborMona,2
    # NPC_TRUST:LandlordHajjMahmoud,3
    # FACTION_REP:ImbabaCrew,3
    # MESSAGE:You help clean the building. The stairwell shines, and so does your standing.
    -> DONE

*   [Contribute supplies instead of labor]
    You buy extra soap, bleach, and garbage bags and leave them at the bottom of the stairs with a note that says "from 4B." It costs money you cannot afford, but it also costs less energy than you do not have, and the gesture lands with the same weight as scrubbing.
    # MONEY:-12
    # STRESS:-2
    # NPC_TRUST:NeighborMona,1
    # MESSAGE:You fund the cleanup rather than sweat through it. The building appreciates both.
    -> DONE

=== event_rooftop_tea ===
The invitation arrives through Mona, who says Umm Karim is hosting tea on the roof tonight and specifically mentioned your name. This is either an honor or a test, and with Umm Karim, the difference is not always clear.

The rooftop at dusk is beautiful in the way that only Cairo rooftops can be: water tanks silhouetted against a sky the color of bruised fruit, the call to prayer mixing with car horns, and the entire city spread out below like a problem too large to solve but too present to ignore.

*   [Attend and listen carefully]
    Umm Karim pours tea with the precision of someone measuring more than liquid. The conversation circles around prices, police movements, and which kiosk owner is cheating his customers this week. You listen more than you speak, which is exactly what Umm Karim prefers.
    # STRESS:-2
    # NPC_TRUST:FixerUmmKarim,3
    # MESSAGE:Umm Karim's rooftop tea is part social call, part intelligence briefing.
    -> DONE

*   [Attend and share what you know]
    You tell Umm Karim about the checkpoint pattern you noticed in Dokki, about the new faces at the depot, about the landlord's mood this week. She listens with absolute stillness, which is how you know she is filing every word.
    # STRESS:-1
    # NPC_TRUST:FixerUmmKarim,5
    # MESSAGE:You trade information at Umm Karim's tea. The currency of the roof is trust.
    -> DONE

=== event_mulid ===
The mulid takes over three blocks of Imbaba with tents, lights, music loud enough to reorganize your heartbeat, and the particular energy of a city celebrating a saint who has been dead for centuries but still draws a bigger crowd than any living person.

The smell of fried dough, incense, and animal sweat mingles in the narrow lanes. Children run with spinning tops. A tent full of women is singing zikr with a fervour that makes the canvas walls tremble. Somewhere, a pickpocket is having the best night of his career.

*   [Join the celebration]
    You buy a bag of semsemya candy, watch a dervish spin until you feel dizzy by proxy, and dance for fifteen minutes in a circle of women who do not know your name but welcome you anyway. Your mother sits on a folding chair watching the lights with an expression that might be joy or might be memory.
    # MONEY:-8
    # STRESS:-10
    # NPC_TRUST:NeighborMona,2
    # MOTHER_HEALTH:2
    # MESSAGE:The mulid swallows you in noise and light. For one night, survival becomes celebration.
    -> DONE

*   [Set up a small stall to sell tea]
    You borrow a thermos and a tray, buy tea wholesale, and set up on the edge of the crowd where the music is slightly less deafening. Three hours of pouring and collecting coins later, you have made enough to cover rent for two days and your feet have gone numb from standing.
    # MONEY:25
    # ENERGY:-10
    # STRESS:-4
    # MESSAGE:You sell tea at the mulid. The profit is modest, the fatigue is not.
    -> DONE

*   [Stay close to your mother and enjoy the spectacle]
    You find a spot on a wall where your mother can sit and watch the procession without being jostled. She points at the horses, laughs at the acrobat, and eats a piece of basbousa that a stranger pressed into her hand. By the end of the evening, she looks younger, or maybe you have just forgotten what she looks like when she is not worried.
    # MONEY:-5
    # STRESS:-6
    # MOTHER_HEALTH:3
    # MESSAGE:You watch the mulid together. Your mother smiles for the first time in weeks.
    -> DONE

=== event_street_argument ===
The shouting starts before noon. Two men from different factions standing in the middle of the market, voices rising over a territory dispute that nobody can fully explain but everybody understands. The women pull their children closer. The shopkeepers close their shutters halfway. The argument is not really about territory. It is about who gets to decide what territory means.

*   [Leave the area immediately]
    You take the long way home, three blocks out of your path, but away from the voices that have started to attract a crowd. In Cairo, crowds around arguments have a way of becoming crowds around incidents, and incidents have a way of becoming statistics that nobody reads.
    # STRESS:3
    # MESSAGE:A street argument erupts. You leave before it becomes something worse.
    -> DONE

*   [Check on Mona's children who were playing in the lane]
    You find them behind the ahwa, oblivious and playing with a broken bicycle wheel. You herd them upstairs to Mona, who thanks you with a grip on your arm that communicates more than any words.
    # STRESS:4
    # NPC_TRUST:NeighborMona,3
    # MESSAGE:You make sure the children are safe. Mona's gratitude is written on her arm where she held you.
    -> DONE

=== event_protection_demand ===
Two men from the Imbaba crew appear at the building entrance. They do not enter. They do not need to. They ask for "the monthly contribution" with the casualness of people collecting a legitimate debt. Hajj Mahmoud stands at the top of the stairs with his ledger clutched to his chest and says nothing.

The amount is not enormous. But it is not nothing.

*   [Pay the protection fee]
    You count out the notes and hand them over without drama. The men nod and leave. The building goes back to its business. Hajj Mahmoud catches your eye on the stairs and his expression says: this is the cost of doing nothing, and everyone pays it eventually.
    # MONEY:-20
    # STRESS:3
    # FACTION_REP:ImbabaCrew,-10
    # MESSAGE:You pay the protection fee. The Imbaba crew takes its cut.
    -> DONE

*   [Ask what happens if you refuse]
    One of the men smiles. It is not a friendly smile. He says nothing happens. Nothing at all. Nothing happens to the building's water supply, nothing happens to the electricity, nothing happens to the windows on the ground floor. He says "nothing" three times, and each time it means something different.
    # STRESS:8
    # FACTION_REP:ImbabaCrew,-10
    # MESSAGE:You ask about the consequences of refusal. The answer is written in what they do not say.
    -> DONE

*   [Appeal to Hajj Mahmoud to negotiate]
    The landlord steps forward with the practiced calm of a man who has done this negotiation before. He speaks to the men in a register of Arabic that sounds more like contract law than conversation. The amount drops by a third. The men leave. Hajj Mahmoud says the discount comes out of his patience, not his pocket.
    # MONEY:-12
    # STRESS:5
    # NPC_TRUST:LandlordHajjMahmoud,4
    # FACTION_REP:ImbabaCrew,-5
    # MESSAGE:Hajj Mahmoud negotiates the fee down. The building pays less, but the landlord remembers.
    -> DONE

=== event_alliance_shift ===
The change arrives as a whisper first: the Dokki crew is moving into Imbaba. By afternoon it is a shout. By evening it is a fact. The balance of the neighborhood shifts overnight like sand in a khamsin, and everyone wakes up to discover that the people who controlled yesterday's streets no longer control today's.

New faces appear at the market. Old faces disappear for a few days. Umm Karim sends word through Mona: be careful, be quiet, and do not make plans that assume the street will look the same tomorrow.

*   [Lay low and wait for the dust to settle]
    You stay home, keep your mother inside, and watch the street from behind the curtain like everyone else in the building. The new faction walks through the lane like men measuring a room they have already decided to rent. You make yourself invisible, which in Cairo is both a survival skill and a small surrender.
    # STRESS:6
    # MESSAGE:The territory flips. You stay inside and watch the new power walk your street.
    -> DONE

*   [Seek out the new faction through Umm Karim]
    You send word to Umm Karim that you want to understand the new arrangement. She sends back a time and a corner and a warning: "Understanding costs something. Bring cash for the tea."
    # STRESS:4
    # NPC_TRUST:FixerUmmKarim,2
    # MESSAGE:You reach out to understand the new faction. Umm Karim arranges the introduction.
    -> DONE

=== event_police_crackdown ===
The raids start at dawn. By the time you open your eyes, the street is full of uniforms and plainclothes officers who move through Imbaba with the efficiency of people who have done this before. Doors are knocked on. IDs are checked. The fruit cart is overturned not because it contains anything illegal but because overturning things is part of the performance.

Your building is spared only because the police started at the other end of the block and ran out of morning before they reached your stairwell.

*   [Stay inside and keep your mother calm]
    You close the windows, silence the phone, and sit with your mother in the dim flat while the street outside processes its way through the neighborhood. She grips your hand and does not let go for two hours. You do not pull away.
    # STRESS:8
    # MESSAGE:A police crackdown sweeps Imbaba. You hide inside and wait for the sound of boots to fade.
    -> DONE

*   [Check on vulnerable neighbors]
    You slip downstairs between waves of police movement and check on the old woman on the ground floor and the Sudanese family whose paperwork is not perfectly in order. They are frightened but undisturbed. You bring them water and stand in their doorway like a wall made of neighborliness.
    # STRESS:10
    # NPC_TRUST:NeighborMona,3
    # NPC_TRUST:PharmacistMariam,2
    # MESSAGE:You check on neighbors during the crackdown. The raid passes, but the fear lingers.
    -> DONE

=== event_territory_flip ===
The old crew left overnight. Not dramatically — they simply stopped appearing at their corners, stopped collecting, stopped watching. By morning, new men stood in their places with the casual authority of people who had been planning this moment for weeks.

The market adjusts. Prices shift. The ahwa serves the same tea to different faces. Cairo does not pause for regime change, even the local kind. The city simply recalculates who matters and who does not, and you recalculate with it.

*   [Adapt to the new order]
    You learn the new faces, the new patterns, the new invisible boundaries. The city has always been a map that redraws itself, and you have always been a woman who learns to read the latest version. Survival is not loyalty. Survival is literacy.
    # STRESS:5
    # MESSAGE:The territory flips. You learn the new map and keep walking.
    -> DONE

*   [Connect with the displaced faction through Youssef]
    You find Youssef in the alley behind the depot, looking more nervous than usual. He says the old crew is regrouping, that they will be back, that loyalty matters in a city where everything else is temporary. He asks where you stand.
    # STRESS:6
    # NPC_TRUST:RunnerYoussef,3
    # MESSAGE:Youssef offers a connection to the displaced faction. Choosing sides has costs either way.
    -> DONE

=== event_refugee_solidarity ===
The tension in Imbaba has been rising for days, and the Sudanese families feel it first — the longer stares, the louder whispers, the way a room can go quiet when you enter it. But tonight, the community fights back with the only weapon it has: presence.

The Sudanese families gather in the lane. Not to protest. Not to shout. Just to stand together, visible and numerous, in the place where they live. The women link arms. The men stand behind them. And for one evening, the street remembers that it belongs to everyone who walks it.

*   [Stand with the community]
    You take your place in the line next to Mariam and a woman whose name you do not know but whose children play with your mother's neighbours. The street watches. The street always watches. But tonight, the watching has an audience that watches back, and the balance holds.
    # STRESS:-4
    # NPC_TRUST:PharmacistMariam,5
    # FACTION_REP:ImbabaCrew,-5
    # MESSAGE:You stand with the Sudanese community. The lane holds its breath, and the tension eases.
    -> DONE

*   [Support from inside — bring tea and water]
    You cannot stand in the line — your mother needs you, or your nerve fails you, or the calculation of risk does not come out in favor of visibility. But you carry a tray of tea down the stairs and set it at the edge of the gathering, and the women nod without breaking formation, and the gesture lands where it needs to.
    # STRESS:-1
    # NPC_TRUST:PharmacistMariam,3
    # MESSAGE:You support from the margins. The tea arrives where the courage is.
    -> DONE

=== event_isolation_signal ===
It has been weeks since you attended anything. The rooftop gatherings passed without you. The iftar tables were set without your place. The Friday prayers were said without your voice among them. The building has noticed, and the noticing has turned from concern to distance.

Mona stops by not to invite you, but to ask if you are alright. The question is gentle. The silence that follows your answer is not.

*   [Promise to attend the next gathering]
    You tell her you will. She hears the uncertainty and does not challenge it, because Mona is generous enough to accept promises she knows might not hold. But her eyes say that the building's patience is a resource, and you have been spending it without depositing.
    # STRESS:3
    # NPC_TRUST:NeighborMona,1
    # MESSAGE:You promise to reconnect. Mona's eyes measure the distance between your word and your will.
    -> DONE

*   [Explain that survival takes all your time]
    You tell her the truth: that every hour is spoken for, that the money demands more than it used to, that choosing between a rooftop gathering and an extra shift is not really a choice. She nods, and the nod is understanding, but it is also a wall going up — slowly, brick by missed invitation.
    # STRESS:4
    # NPC_TRUST:NeighborMona,-2
    # MESSAGE:You explain your absence. Mona understands, but understanding has limits.
    -> DONE

=== event_friday_prisoner ===
Friday prayer in Imbaba. The men line up shoulder to shoulder, and the women gather behind. You stand in the back row because the front rows belong to women whose families have been in this building for generations, and you are still learning the texture of belonging.

The imam speaks about patience. About endurance. About the difference between surviving and living. You know the difference. You learned it in a cell, and you are learning it again in a flat that costs more than it gives.

After prayer, no one approaches you. No one avoids you either. In Cairo, acceptance sometimes looks exactly like indifference, and you are learning to read it as progress.
# STRESS:-2
# NPC_TRUST:LandlordHajjMahmoud,1
# MESSAGE:Friday prayer as a released prisoner. The community does not embrace you, but it does not push you away.
-> DONE

=== event_friday_medical ===
At the Friday gathering, a child scrapes his knee on the water tank. The women look around for someone who knows what to do, and your hand moves before your hesitation catches up. You clean the wound with water, apply pressure, and bandage it with a strip of cloth from your pocket.

The women watch with the particular attention that Cairo reserves for people who reveal unexpected competence. One of them says you have steady hands. Another says the clinic is always hiring.

Your hands remember what your career forgot.
# STRESS:-3
# NPC_TRUST:NurseSalma,2
# NPC_TRUST:NeighborMona,2
# MESSAGE:You treat a child's wound at the Friday gathering. Your medical training finds uses the degree never did.
-> DONE
