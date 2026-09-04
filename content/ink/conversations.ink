# Ordinary recurring conversations are composed from authored openers and bodies.
# Core selects a stable context and a non-repeating opener/body pair.

=== recurring_conversation ===
{conversation_variant == "":
This contact is waiting for the conversation details to arrive through the repaired handset or a face-to-face introduction.
}

{conversation_npc == "LandlordHajjMahmoud":
Hajj Mahmoud taps the ledger with one finger and waits for the answer behind your answer.
}
{conversation_npc == "FixerUmmKarim":
Umm Karim watches the lane before she watches you. Her advice arrives trimmed of anything that could be repeated carelessly.
}
{conversation_npc == "OfficerKhalid":
Officer Khalid keeps his voice level and his questions narrow, as if both are forms of paperwork.
}
{conversation_npc == "NeighborMona":
Mona brings the conversation into the stairwell where neighbors can pass without pretending to listen.
}
{conversation_npc == "NurseSalma":
Nurse Salma writes one last note, caps her pen, and gives you the attention the clinic has not given her all day.
}
{conversation_npc == "WorkshopBossAbuSamir":
Abu Samir wipes metal dust from his hands and looks at the broken thing between you as if it has a story.
}
{conversation_npc == "CafeOwnerNadia":
Nadia sets down two glasses of tea and charges neither one to the account, which is its own kind of opening.
}
{conversation_npc == "FenceHanan":
Hanan chooses a corner with two exits and speaks in the careful language of markets that remember faces.
}
{conversation_npc == "RunnerYoussef":
Youssef gives the street one more glance before he gives you a sentence. He trusts timing more than promises.
}
{conversation_npc == "PharmacistMariam":
Mariam checks the medicine shelf, then your face, measuring what can be said without making your mother's need public.
}
{conversation_npc == "DispatcherSafaa":
Safaa talks over the depot noise with the exactness of someone who has learned which instructions survive a bad connection.
}
{conversation_npc == "LaundryOwnerIman":
Iman folds a shirt while she listens, making the ordinary work carry the weight of the conversation.
}
{conversation_npc == "VendorTarek":
Tarek leans against the cart and smiles without promising that the day will become easier.
}

{conversation_npc == "NeighborMona" && conversation_context == "helped": Mona remembers the bread you carried upstairs when her knees were bad. She does not call it a favor; she simply puts your name back on the first warning list.}
{conversation_npc == "NurseSalma" && conversation_context == "debt_warm": Salma remembers the debt without turning it into a sermon. Her trust is practical: she tells you which medicine can wait and which cannot.}
{conversation_context == "recent_refusal": The refused favor remains in the room. Nobody raises it, but the next kindness is measured before it is offered.}
{conversation_context == "heat": The protection money and the police attention have reached the same conversation. There is no clean way to pretend they are separate.}

{conversation_context == "default": The subject is ordinary until one detail makes it less so.}
{conversation_context == "warm": Trust makes room for a little honesty, not a guarantee.}
{conversation_context == "cold": The silence between sentences has become part of the negotiation.}
{conversation_context == "hot": Everyone is speaking softly because the street is listening.}
{conversation_context == "lean": Money is present in the conversation even when nobody names it.}
{conversation_context == "urgent": The practical need comes first; feelings wait outside the door.}
{conversation_context == "broke": The week's arithmetic has reached the room before you have.}
{conversation_context == "broke_soft": Concern is offered carefully, so it does not sound like pity.}
{conversation_context == "hostile": The relationship has a memory, and neither of you is willing to forget it.}
{conversation_context == "double_life": Two versions of your week sit between the words.}
{conversation_context == "trusted": An old favor gives the conversation a longer horizon.}
{conversation_context == "recent_refusal": The last refusal is still close enough to cast a shadow.}
{conversation_context == "repeat": Familiarity makes small changes easier to notice.}
{conversation_context == "first": You are still learning which questions are safe to ask.}
{conversation_context == "marked": A record exists somewhere, whether or not either of you can see it.}
{conversation_context == "heat": The city has started connecting your money to your company.}
{conversation_context == "helped": Something you did remains in the room with you.}
{conversation_context == "debt": Help has a balance, and both of you know who is carrying it.}
{conversation_context == "debt_warm": Gratitude and obligation have become difficult to separate.}
{conversation_context == "suspicious": The honest route and the dangerous route keep appearing in the same sentence.}
{conversation_context == "embarrassed": A past mistake is being handled without being forgiven.}
{conversation_context == "embedded": The connection has become useful enough to be dangerous.}
{conversation_context == "regular": Repetition has made the work faster, not safer.}

{conversation_opener == 1: The first thing discussed is the water schedule and who was awake when the pump finally started.}
{conversation_opener == 2: You begin with the price of food, then admit that the price is not the part worrying you most.}
{conversation_opener == 3: A delivery drone stalls above the roof, giving the conversation a few seconds of mechanical silence.}
{conversation_opener == 4: Someone uses the phrase ya zalameh nearby, affectionate or irritated depending on who hears it.}
{conversation_opener == 5: The subject is a cracked handset whose repair has already cost more than its replacement.}
{conversation_opener == 6: You mention the hotter nights and the room answers with the sound of a tired fan.}
{conversation_opener == 7: A rumor from Dokki arrives without a source and leaves without becoming a fact.}
{conversation_opener == 8: The conversation turns to work hours moved before sunrise because concrete stores too much heat.}
{conversation_opener == 9: Somebody asks after your mother, and the question is kind enough to make the answer difficult.}
{conversation_opener == 10: You both look toward the lane when a patrol siren changes pitch and then fades.}

{conversation_body == 1: You answer plainly. It does not solve anything, but it gives the other person something real to work with.}
{conversation_body == 2: You make a small joke about Cairo's arithmetic. The laugh is brief, but it is not fake.}
{conversation_body == 3: You ask what the other person has not said. The answer takes time and costs a little trust.}
{conversation_body == 4: You offer one useful fact from the cooperative ledger, keeping names out of it.}
{conversation_body == 5: You say wallahi without making it a performance, and the promise lands as a request for patience.}
{conversation_body == 6: You listen while the other person describes a problem that cannot be fixed by one generous gesture.}
{conversation_body == 7: You disagree respectfully. In a crowded city, disagreement is sometimes proof that the relationship is alive.}
{conversation_body == 8: You trade a practical contact: a repairer, a shaded route, a clinic hour, or a person who answers messages.}
{conversation_body == 9: You leave the decision open, because forcing certainty would make the next conversation impossible.}
{conversation_body == 10: You part with a promise to return, both of you aware that promises require water, money, and time to survive.}

-> DONE
