# Shared infrastructure crisis: the rooftop water-and-power cooperative.

=== crisis_introduction ===
The cooperative's rooftop committee calls a meeting at the pump hour. Three buildings share repaired panels, retired bus cells, and a cooling room whose key is held by people rather than a platform.

An automated irregular-consumer review has flagged the block. The model is narrow, fed by incomplete meter records, and backed by an office that would rather trust a score than visit Imbaba.

*   [Ask who keeps the records]
    Hajj Mahmoud brings out paper ledgers while Mona opens the neighborhood mesh on her repaired handset. The appeal will need both kinds of evidence.
    # MESSAGE:The rooftop cooperative has become the month's shared problem.
    -> DONE

*   [Offer to help with the pump roster]
    You take the late pump shift and learn which families are already rationing twice. The cooperative is not an idea. It is buckets, batteries, and people who remember.
    # STRESS:-2
    # MESSAGE:You join the cooperative's work before you know what it will cost.
    -> DONE

=== crisis_classification ===
The review arrives as a clean notification: access downgraded pending verification. The score does not know whose mother needs refrigerated medicine or which meter has been repaired three times.

*   [Gather meter evidence]
    You photograph old readings, copy pump hours, and ask the repair crew for their work slips. The evidence is ordinary. That is why it might survive an appeal.
    # CRISIS_EVIDENCE:1
    # MESSAGE:You begin an evidence file for the cooperative's appeal.
    -> DONE

*   [Ask a human clerk to review the score]
    A clerk agrees to look, but only if someone brings a stamped form from the district office. The machine has created work for people who already have too much.
    # CRISIS_EVIDENCE:1
    # STRESS:3
    # MESSAGE:The appeal gains a human route, and a new queue.
    -> DONE

=== crisis_appeal ===
The appeal window opens. Salma can document medicine storage, Mahmoud can attest to the meters, and Umm Karim knows which office answers a polite question and which one answers only to pressure.

*   [Build the evidence appeal]
    You carry copies between the clinic, the roof, and the district office. It takes a day and costs transport, but the file becomes difficult to dismiss as a complaint from nowhere.
    # CRISIS_DECISION:EvidenceAppeal
    # CRISIS_RESOURCES:5
    # MESSAGE:You choose documentation and a formal appeal.
    -> DONE

*   [Organize a mutual-aid roster]
    Mona turns the pump schedule into a rota. Families trade cooling hours, medicine storage, and shade. It cannot change the score, but it can keep people alive while the score is argued over.
    # CRISIS_DECISION:MutualAid
    # CRISIS_RESOURCES:4
    # MESSAGE:You choose a neighborhood response built from shared care.
    -> DONE

=== crisis_heat_emergency ===
The nights stay hot. The water allocation is still downgraded, the battery bank is tired, and every solution has a human cost. The cooperative asks what you can protect without pretending you can protect everything.

*   [Repair the shared storage]
    You spend parts and a day's work on the oldest battery rack. It will not make power abundant. It may keep the cooling room open through the worst hours.
    # CRISIS_RESOURCES:8
    # ENERGY:-8
    # MESSAGE:The shared battery holds a little longer because people repaired it.
    -> DONE

*   [Ask for public pressure]
    Salma and Mahmoud prepare statements while Umm Karim finds witnesses. The office may respond to attention, but attention also puts names on a list.
    # CRISIS_DECISION:PublicPressure
    # STRESS:5
    # MESSAGE:The cooperative makes its case in public and accepts the exposure.
    -> DONE

=== crisis_commitment ===
By day twenty-five, waiting is its own decision. A quiet appeal, a mutual-aid network, and a diverted access key can each keep someone cool. None can keep everyone safe forever.

*   [Stay with the formal appeal]
    You put the remaining transport money into the stamped file and refuse the shortcut. It is slower, legible, and vulnerable to the office deciding that poor people can wait.
    # CRISIS_DECISION:EvidenceAppeal
    # CRISIS_RESOURCES:6
    # MESSAGE:You commit to the formal appeal.
    -> DONE

*   [Protect the neighborhood through mutual aid]
    You give the roster your time and the last of the spare filters. The roof becomes a little more governed by people who live beneath it, and a little less protected by official permission.
    # CRISIS_DECISION:MutualAid
    # CRISIS_RESOURCES:7
    # MESSAGE:You commit to a shared emergency plan.
    -> DONE

*   [Take the diverted access key]
    The key would restore a better allocation for a week. It would also leave a trail through a system that already marks your neighborhood as irregular.
    # CRISIS_DECISION:Diversion
    # CRISIS_RESOURCES:3
    # POLICE:5
    # MESSAGE:You take an illicit shortcut and accept the exposure.
    -> DONE

=== crisis_resolution ===
Day thirty arrives with no clean victory. The office has answered, the roof has changed, and the people who helped now know what you were willing to risk.

*   [Resolve through the cooperative]
    You put the final decision to the committee. The result is imperfect but shared: water rosters, repair duties, and cooling hours are written where everyone can see them.
    # CRISIS_RESOLUTION:SharedEmergencyPlan
    # MESSAGE:The cooperative survives by becoming more accountable to its residents.
    -> DONE

*   [Accept restricted access and protect your household]
    You take the narrower allocation and reserve what remains for your mother. The building loses comfort, but your household gets a little certainty.
    # CRISIS_RESOLUTION:AccessRestricted
    # MESSAGE:Your household gains certainty while the wider block carries the shortage.
    -> DONE
