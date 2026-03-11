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