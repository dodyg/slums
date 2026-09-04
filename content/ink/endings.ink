# Ending scenes

=== ending_commitment ===
The month has narrowed the future to one decision. The choice is not a promise that Cairo will become kind. It is a decision about what you will spend, protect, or leave behind when the next morning arrives.

{pending_ending != "StabilityHonestWork" && pending_ending != "NetworkShelter" && pending_ending != "QuitTheLuxorDream" && pending_ending != "CrimeKingpin":
The final commitment is waiting for an eligible ending to be selected.
-> DONE
}

{pending_ending == "StabilityHonestWork":
    *   [Keep the clinic hours and reserve the last money for your mother's care]
        You choose care over a faster escape. The work will remain tiring and underpaid, but the decision keeps a reliable hand beside the people who cannot afford a second mistake.
        # ENDING_COMMIT:StabilityHonestWork,care_shift
        -> DONE
    *   [Take the better depot contract and let the building share the care]
        You choose the contract. Mona and Salma hear the request for help without treating it as a moral test, but your household becomes dependent on a network that is already carrying too much.
        # ENDING_COMMIT:StabilityHonestWork,depot_contract
        -> DONE
}
{pending_ending == "NetworkShelter":
    *   [Give your remaining savings to the cooling-room fund]
        You put the money where everyone can see it. The fund protects more than your flat, and it also means there is no private reserve when the next shortage arrives.
        # ENDING_COMMIT:NetworkShelter,cooling_fund
        -> DONE
    *   [Keep the savings and accept the roster's help]
        You keep a small private buffer. The roster still includes your household, but the people who built it remember that shelter is a contribution before it is a service.
        # ENDING_COMMIT:NetworkShelter,private_buffer
        -> DONE
}
{pending_ending == "QuitTheLuxorDream":
    *   [Buy the tickets and leave the Cairo network behind]
        You spend the money on distance. It is freedom with a thin wallet, and every person you leave behind has to decide whether goodbye is a wound or a boundary.
        # ENDING_COMMIT:QuitTheLuxorDream,buy_distance
        -> DONE
    *   [Delay the train and send one last payment home]
        You send help south before you buy your own passage. The train still exists, but the departure becomes later and less certain.
        # ENDING_COMMIT:QuitTheLuxorDream,send_home
        -> DONE
}
{pending_ending == "CrimeKingpin":
    *   [Protect the clinic shipment and lose the easy money]
        You keep one route from becoming another missing medicine story. The people above you call it sentiment. The people below you call it a debt they may remember.
        # ENDING_COMMIT:CrimeKingpin,protect_shipment
        -> DONE
    *   [Take control of the shipment and make the network pay]
        You choose power with open eyes. The cooperative gets no apology, only a new account ledger with your name on it.
        # ENDING_COMMIT:CrimeKingpin,control_shipment
        -> DONE
}

=== ending_destitution ===
There is a point where the city stops offering you choices and starts offering you corners. The water roster is posted behind the pump room, and your name has moved to the bottom because you cannot pay your share. The cooling-room queue fills before dawn. Your mother's rationing makes every quiet cough sound like an accusation. You are still alive, but life has narrowed to borrowed shade, shut doors, and one more day from people who have none to spare.
-> DONE

=== ending_destitution_medical ===
The clinic recognizes your old training before it recognizes your inability to pay. Salma lets you stand beside the triage screen, but the cooling-room queue is full and your mother's name is still waiting for a medicine allocation. You know exactly which readings are dangerous. You cannot turn that knowledge into a dose, a fan, or a place in the water roster. The work you once imagined becomes another thing you cannot afford to reach.
-> DONE

=== ending_destitution_prisoner ===
The file with your name on it does not feed anyone. At the pump, a neighbor checks the ration list twice before saying the storage room is full. Your mother folds her portion smaller so yours can look like a meal. By afternoon the cooling-room queue has closed, and the streets are too hot to wait outside it. The state took years from you; poverty takes the remaining hours one careful refusal at a time.
-> DONE

=== ending_destitution_sudanese ===
The kitchen committee keeps your place as long as it can, but the food parcel is smaller this week and the water roster has no spare slot. Your mother saves the last cool water for you, then denies doing it. At the cooling room, the queue has already curled around the building. Cairo continues to call your household temporary while the years accumulate, and hunger makes even belonging feel like another account you cannot settle.
-> DONE

=== ending_mother_died ===
After the room goes quiet, Cairo sounds indecently normal. Pots clang in the stairwell. Someone downstairs argues over bread. A microbus horn cuts through the afternoon as if nothing in your flat has been divided into before and after.

You bury your mother with help, debt, and the stunned courtesy people reserve for grief they know will not save you. When the mourners leave, the apartment feels smaller than poverty ever made it. Whatever comes next has to be built around an absence you could not bargain with.
-> DONE

=== ending_stability ===
Stability is rent paid on time, bread on the table, water stored before the pump cuts, and a place in the building's cooling roster when the nights stay hot. Mornings no longer begin with panic already in your throat. That is enough to build on.
-> ending_crisis_reflection

=== ending_stability_medical ===
The work is narrower, poorer, and less admired than the white coat you once imagined. Your hands are still useful. Salma trusts you to recognize heat illness before the triage model does, and your mother trusts you with the ordinary care that keeps her alive.
-> ending_crisis_reflection

=== ending_stability_prisoner ===
Stability does not erase the file with your name on it or the years already taken. It does something smaller and harder: it proves the state did not get to define every room you would stand in afterward.
-> DONE

=== ending_stability_sudanese ===
The life you build is still called temporary by people who have never watched years harden around a {gender == "male": man | woman}. Even so, the rent gets paid and your mother eats. The Sudanese kitchen puts your name on its water and food committee. A future begins to exist in Cairo without asking permission.
-> DONE

=== ending_luxor ===
The train south feels unreal at first. Even after Giza falls behind, you keep waiting for Cairo to pull you back by the wrist.

Luxor is hotter. Everyone who promised an easy escape left that part out. Work begins before sunrise, stops when the stone streets turn white with heat, and resumes after maghrib. Water and cooling still cost money. What changes is the shape of the life around those costs: relatives nearby, a room you can keep, and enough distance from the old routes to choose what your family becomes.
-> DONE

=== ending_luxor_medical ===
In Luxor, nobody mistakes you for the student you were supposed to become. That is painful and freeing at once. A neighborhood clinic gives you morning hours during the hottest months. What survives is the discipline: clean hands, careful doses, heat checks for older patients, and a refusal to let care become another broken promise.
-> DONE

=== ending_luxor_prisoner ===
Leaving for Luxor does not erase the state file with your name on it. It does something quieter and still radical: it forces distance between your future and the rooms where other people learned to speak about you as if a database were fate.
-> DONE

=== ending_luxor_sudanese ===
The train south moves toward a different kind of scrutiny, not an escape from it. Luxor is not free of paperwork or the vocabulary people use for {gender == "male": men | women} they still call temporary. Sudanese, Nubian, and Upper Egyptian contacts help your household find a room, work around the hottest hours, and enter a savings circle. Ordinary life remains difficult, but it becomes possible.
-> DONE

=== ending_arrested ===
The holding cell smells of heat, metal, and old fear. An officer keeps asking you to press a thumb to a cracked scanner. Somewhere above you, Cairo keeps bargaining, praying, hustling, and surviving without pausing long enough to notice one more {gender == "male": man | woman} disappear indoors.
-> DONE

=== ending_eviction ===
Seven days behind on rent turns a home into a countdown. By the time Hajj Mahmoud stops knocking and starts ordering, the room has already become a pile of things that cannot protect you.

You and your mother carry what you can into the stairwell while the building pretends not to watch. Outside, the shaded public rooms are already full and the afternoon pavement is dangerous. Cairo has many ways to make a {gender == "male": man | woman} poor. Losing a room now also means losing water storage, cooling, and a door against the heat.
-> DONE

=== ending_network_shelter ===
You stay in the city and become difficult to erase inside it. A neighbor warns you before trouble climbs the stairs. A nurse stretches medicine one more week. The rooftop committee saves a share of battery power for your mother's cooling unit, and the water roster includes your household even when you cannot contribute. It is not safety. It is shelter built out of people.
-> ending_crisis_reflection

=== ending_network_shelter_mona ===
You stay afloat because Mona keeps turning the building into a line of warning, gossip, and quiet mercy. The city does not soften. It simply reaches you through a neighbor before it reaches you through a threat, and over time that difference becomes the shape of survival.
-> DONE

=== ending_network_shelter_salma ===
You stay afloat partly because Salma never lets hardship become abstract. A dosage reminder, a clinic favor, one honest answer at the right hour: none of it is dramatic, but together it keeps your life from narrowing into panic.
-> DONE

=== ending_network_shelter_nadia ===
You stay afloat because Nadia keeps your name circulating in the places where women barter reputation, warning, and opportunity between cups of tea. She cannot make Cairo kind. She can make sure it does not forget you are still worth calling.
-> DONE

=== ending_network_shelter_hanan ===
You stay afloat because Hanan knows how to survive a city built on selective memory. She teaches you no morality, offers no illusion of innocence, only the harder lesson that some people keep each other alive by deciding exactly what not to repeat.
-> DONE

=== ending_crime_kingpin ===
The money finally arrives in amounts that change how people look at you. Doors open faster. Favors come wrapped as respect. Even the ones above you speak more carefully now.

But every rung you climbed belongs to someone else too: stolen delivery routes, hijacked fleet accounts, clinic power cells that should have kept a diagnostic fridge alive. The errands are bigger, the witnesses closer, the punishment for hesitation clearer. From the outside it looks like power. Up close it is only a better-lit cage, and Cairo knows exactly how to keep the key.
-> ending_crisis_reflection

=== ending_crisis_reflection ===
{crisis_resolution_state == "SharedEmergencyPlan":
The rooftop cooperative remains imperfect but answerable to its residents. Your ending carries the memory of water rosters, repaired storage, and a decision made in public.
}
{crisis_resolution_state == "CooperativeProtected":
The cooperative holds together because people protected access before the shortage could be turned into a private advantage.
}
{crisis_resolution_state == "AccessRestricted":
Your household has a narrower certainty, while the block continues to carry the cost of the restricted allocation.
}
{crisis_resolution_state == "DivertedAndExposed":
The diverted access left a trail. The cooperative has power for now, but names, permits, and police attention will outlast the relief.
}
{crisis_resolution_state == "Unresolved":
The cooperative's crisis remains unresolved, another pressure waiting beyond the month you managed to survive.
}
-> DONE
