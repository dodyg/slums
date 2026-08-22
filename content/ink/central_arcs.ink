# The five central-character beats are deliberately short and choice-driven. Their decisions
# remain in Core so later scenes can acknowledge what was actually chosen.

=== central_mother_arc ===
Your mother waits until the room is cool enough to speak without wasting breath. She does not ask whether you can save her. She asks what you are willing to tell the neighbors about the household's need.
*   [Let Mona add your flat to the medicine and cooling rota]
    You tell her the truth will travel. She says truth already travels; the question is whether it arrives carrying help.
    # CENTRAL_DECISION:Mother,MotherAcceptCare
    # MOTHER_HEALTH:2
    # NPC_TRUST:NeighborMona,1
    # FLAG:central_mother_arc_seen
    -> DONE
*   [Keep the household's medical needs private]
    You promise to manage it quietly. She accepts the promise, then asks you to stop calling privacy a plan when it is really fear of being seen needing people.
    # CENTRAL_DECISION:Mother,MotherKeepPrivate
    # STRESS:2
    # FLAG:central_mother_arc_seen
    -> DONE

=== central_mona_arc ===
Mona has turned the stairwell into a noticeboard without putting up a single screen. Her question is simple: will your household share its pump hours, even on the days you have paid for them yourself?
*   [Share your paid pump hours with the building]
    Mona writes the hours in pencil. She does not call it generosity. She calls it a way to keep the roof from becoming five separate emergencies.
    # CENTRAL_DECISION:NeighborMona,MonaShareRota
    # CRISIS_RESOURCES:3
    # NPC_TRUST:NeighborMona,3
    # FLAG:central_mona_arc_seen
    -> DONE
*   [Keep the hours as a private reserve]
    Mona nods and does not argue. The next time she brings a warning, she leaves the message at your door instead of knocking.
    # CENTRAL_DECISION:NeighborMona,MonaKeepReserve
    # NPC_TRUST:NeighborMona,-1
    # FLAG:central_mona_arc_seen
    -> DONE

=== central_salma_arc ===
Salma has evidence that the allocation review is misreading medicine storage. Publishing it could force an answer. It could also expose which patients cannot afford reliable cooling.
*   [Publish the evidence with names removed]
    Salma agrees to the public record, but makes you read every line twice. A useful document is still capable of harming the people it describes.
    # CENTRAL_DECISION:NurseSalma,SalmaPublishEvidence
    # CRISIS_EVIDENCE:2
    # NPC_TRUST:NurseSalma,2
    # FLAG:central_salma_arc_seen
    -> DONE
*   [Protect the patients and keep the evidence private]
    Salma exhales. The appeal becomes slower, but the clinic's poorest families remain people rather than examples.
    # CENTRAL_DECISION:NurseSalma,SalmaProtectPatient
    # CRISIS_EVIDENCE:1
    # NPC_TRUST:NurseSalma,3
    # FLAG:central_salma_arc_seen
    -> DONE

=== central_mahmoud_arc ===
Hajj Mahmoud brings the building ledger to the roof. He can open the accounts and let residents challenge every pump and battery decision, or protect the building's reputation by keeping disputes inside his office.
*   [Open the ledger to the residents]
    He hates the idea, which is why he takes it seriously. The first argument begins before the first page is flat.
    # CENTRAL_DECISION:HajjMahmoud,MahmoudOpenLedger
    # CRISIS_EVIDENCE:1
    # NPC_TRUST:LandlordHajjMahmoud,1
    # FLAG:central_mahmoud_arc_seen
    -> DONE
*   [Keep the disputes private]
    He closes the ledger. The roof stays quieter, and nobody learns how close the accounts are to failure.
    # CENTRAL_DECISION:HajjMahmoud,MahmoudProtectReputation
    # NPC_TRUST:LandlordHajjMahmoud,2
    # STRESS:1
    # FLAG:central_mahmoud_arc_seen
    -> DONE

=== central_ummkarim_arc ===
Umm Karim knows which office will answer a public complaint and which one will quietly add a household to a watch list. She offers a warning, but asks whether you want the whole roof to know its source.
*   [Share the warning through the neighborhood network]
    The warning moves in ordinary language: a tea invitation, a pharmacy question, a message carried by someone who was already going that way. It is harder to erase because nobody owns it alone.
    # CENTRAL_DECISION:UmmKarim,UmmKarimShareWarning
    # CRISIS_EVIDENCE:1
    # POLICE:2
    # NPC_TRUST:FixerUmmKarim,2
    # FLAG:central_ummkarim_arc_seen
    -> DONE

*   [Limit the warning to your household]
    Umm Karim accepts the boundary. She also stops offering information that might become your responsibility later.
    # CENTRAL_DECISION:UmmKarim,UmmKarimLimitExposure
    # NPC_TRUST:FixerUmmKarim,-1
    # FLAG:central_ummkarim_arc_seen
    -> DONE

=== central_mother_vulnerability ===
Your mother admits she hid a missed dose because she did not want to become the reason you lost a shift. The admission is practical, not sentimental: concealment has already changed the household's risk.
*   [Ask Salma for a monitored medicine plan]
    You accept that care can be shared without becoming a surrender of dignity.
    # CENTRAL_DECISION:Mother,MotherAcceptCare
    # NPC_TRUST:NurseSalma,1
    # MOTHER_HEALTH:2
    # FLAG:central_mother_vulnerability_seen
    -> DONE
*   [Keep managing the doses yourself]
    You take the schedule back into your own hands. Your mother agrees, then asks who will watch you when you are the one who fails.
    # CENTRAL_DECISION:Mother,MotherKeepPrivate
    # STRESS:2
    # FLAG:central_mother_vulnerability_seen
    -> DONE

=== central_mother_conflict ===
Your mother hears that money came through a route you have not explained. She will not accuse you. She will ask whether the money lets you sleep.
*   [Tell her where the money came from]
    The truth hurts, but it gives her a chance to object before the debt becomes hers too.
    # CENTRAL_DECISION:Mother,MotherAcceptCare
    # NPC_TRUST:NeighborMona,1
    # STRESS:1
    # FLAG:central_mother_conflict_seen
    -> DONE
*   [Protect her from the details]
    You call silence kindness. She calls it a decision made without her.
    # CENTRAL_DECISION:Mother,MotherKeepPrivate
    # NPC_TRUST:NeighborMona,-1
    # STRESS:3
    # FLAG:central_mother_conflict_seen
    -> DONE

=== central_mother_reckoning ===
The heat leaves both of you too tired for performance. Your mother asks for one thing she can still decide: who is allowed to know when the flat needs help.
*   [Let her choose the people who are told]
    She chooses carefully. Agency is not the same as safety, but it is still hers.
    # CENTRAL_DECISION:Mother,MotherAcceptCare
    # NPC_TRUST:NeighborMona,2
    # MOTHER_HEALTH:1
    # FLAG:central_mother_reckoning_seen
    -> DONE
*   [Insist that you will carry it alone]
    She lets you speak, then turns her face toward the wall until the argument ends.
    # CENTRAL_DECISION:Mother,MotherKeepPrivate
    # STRESS:3
    # FLAG:central_mother_reckoning_seen
    -> DONE

=== central_mother_outcome ===
Your mother is not a meter reading. She has a preference, a memory of every promise, and a vote in what the household becomes.
*   [Follow the care plan she helped design]
    She corrects one detail, approves another, and tells you to stop mistaking obedience for listening.
    # CENTRAL_DECISION:Mother,MotherAcceptCare
    # MOTHER_HEALTH:2
    # FLAG:central_mother_outcome_seen
    -> DONE
*   [Keep the plan private between the two of you]
    She accepts the boundary with a tired nod, but the wider network no longer assumes it may step in.
    # CENTRAL_DECISION:Mother,MotherKeepPrivate
    # NPC_TRUST:NeighborMona,-1
    # FLAG:central_mother_outcome_seen
    -> DONE

=== central_mona_transaction ===
Mona asks you to carry the water roster while she takes a neighbor to the clinic. The request is work, not a test of affection.
*   [Take the roster]
    You learn which floors argue about fairness and which floors simply run out first.
    # CENTRAL_DECISION:NeighborMona,MonaShareRota
    # CRISIS_RESOURCES:2
    # NPC_TRUST:NeighborMona,2
    # FLAG:central_mona_transaction_seen
    -> DONE
*   [Say you cannot take another obligation]
    Mona accepts the refusal and finds another hand. She remembers that you answered plainly.
    # CENTRAL_DECISION:NeighborMona,MonaKeepReserve
    # NPC_TRUST:NeighborMona,-1
    # FLAG:central_mona_transaction_seen
    -> DONE

=== central_mona_vulnerability ===
Mona's hands shake while she counts the building's remaining water credits. She has been hiding that the children on her floor are already skipping baths.
*   [Help her publish the shortage]
    The notice makes the building uncomfortable and makes it harder for an office to pretend nobody knew.
    # CENTRAL_DECISION:NeighborMona,MonaShareRota
    # CRISIS_EVIDENCE:1
    # NPC_TRUST:NeighborMona,2
    # FLAG:central_mona_vulnerability_seen
    -> DONE
*   [Offer your private reserve instead]
    The floor gets one better day. Mona worries that the gift will make the system less visible.
    # CENTRAL_DECISION:NeighborMona,MonaKeepReserve
    # MONEY:-5
    # NPC_TRUST:NeighborMona,1
    # FLAG:central_mona_vulnerability_seen
    -> DONE

=== central_mona_conflict ===
Mona learns you withheld a pump hour. Her anger is not about the hour. It is about being asked to govern a shortage with incomplete facts.
*   [Show her the household ledger]
    The apology begins with numbers and ends with a harder admission: you wanted one thing that was only yours.
    # CENTRAL_DECISION:NeighborMona,MonaShareRota
    # NPC_TRUST:NeighborMona,2
    # CRISIS_EVIDENCE:1
    # FLAG:central_mona_conflict_seen
    -> DONE
*   [Keep the ledger closed]
    Mona stops asking you to mediate. The building remains functional, but your place in its decisions gets smaller.
    # CENTRAL_DECISION:NeighborMona,MonaKeepReserve
    # NPC_TRUST:NeighborMona,-3
    # FLAG:central_mona_conflict_seen
    -> DONE

=== central_mona_outcome ===
Mona chooses a new rota rule: no household is asked to contribute what it cannot replace, and every exception is written down.
*   [Sign your name beside the rule]
    Your name becomes accountable rather than important.
    # CENTRAL_DECISION:NeighborMona,MonaShareRota
    # CRISIS_RESOURCES:2
    # NPC_TRUST:NeighborMona,2
    # FLAG:central_mona_outcome_seen
    -> DONE
*   [Leave the rule to the committee]
    The committee carries on. Mona wishes you had joined it, but she does not turn the wish into punishment.
    # CENTRAL_DECISION:NeighborMona,MonaKeepReserve
    # FLAG:central_mona_outcome_seen
    -> DONE

=== central_salma_transaction ===
Salma asks you to check a medicine cooler whose sensor is reporting impossible temperatures. Your medical training can help, but your name on the repair log may expose the clinic.
*   [Sign the repair log]
    The cooler stays in service and the record becomes evidence that a person noticed the failure.
    # CENTRAL_DECISION:NurseSalma,SalmaPublishEvidence
    # CRISIS_EVIDENCE:1
    # NPC_TRUST:NurseSalma,2
    # FLAG:central_salma_transaction_seen
    -> DONE
*   [Repair it without signing]
    The medicine is safer tonight, but the institution learns nothing about who fixed it or why.
    # CENTRAL_DECISION:NurseSalma,SalmaProtectPatient
    # NPC_TRUST:NurseSalma,1
    # FLAG:central_salma_transaction_seen
    -> DONE

=== central_salma_vulnerability ===
Salma confesses that the triage service classified an elderly patient as low priority because it could not read a worn patch. She has been correcting it by hand between cases.
*   [Document the model's failure]
    The report may help the next patient and may also make Salma the person blamed for the service's limits.
    # CENTRAL_DECISION:NurseSalma,SalmaPublishEvidence
    # CRISIS_EVIDENCE:2
    # NPC_TRUST:NurseSalma,1
    # FLAG:central_salma_vulnerability_seen
    -> DONE
*   [Keep the case private and fix the queue]
    The patient gets seen. The same failure remains waiting in the next queue.
    # CENTRAL_DECISION:NurseSalma,SalmaProtectPatient
    # MOTHER_HEALTH:1
    # NPC_TRUST:NurseSalma,2
    # FLAG:central_salma_vulnerability_seen
    -> DONE

=== central_salma_conflict ===
Salma sees your name near a public appeal. She asks whether you are trying to help the clinic or make its evidence useful to your own ending.
*   [Let her remove your name from the appeal]
    The document becomes less dramatic and more honest.
    # CENTRAL_DECISION:NurseSalma,SalmaProtectPatient
    # NPC_TRUST:NurseSalma,3
    # FLAG:central_salma_conflict_seen
    -> DONE
*   [Keep your name on it]
    You accept the visibility. Salma accepts the choice, not the explanation.
    # CENTRAL_DECISION:NurseSalma,SalmaPublishEvidence
    # POLICE:1
    # FLAG:central_salma_conflict_seen
    -> DONE

=== central_salma_outcome ===
Salma closes the case file and asks what kind of record you want the clinic to keep after the crisis.
*   [Keep a public failure log]
    The log names broken assumptions without naming patients. It will make future repair harder to hide.
    # CENTRAL_DECISION:NurseSalma,SalmaPublishEvidence
    # CRISIS_EVIDENCE:1
    # NPC_TRUST:NurseSalma,2
    # FLAG:central_salma_outcome_seen
    -> DONE
*   [Keep a private patient-protection log]
    The log follows people rather than institutions. It is safer now and harder for anyone else to audit.
    # CENTRAL_DECISION:NurseSalma,SalmaProtectPatient
    # NPC_TRUST:NurseSalma,1
    # FLAG:central_salma_outcome_seen
    -> DONE

=== central_mahmoud_transaction ===
Hajj Mahmoud asks you to inspect a battery invoice. He does not trust the portal, but he trusts the person willing to read the small print.
*   [Read it with the residents present]
    The invoice takes longer to explain in public, and nobody can later claim it was invisible.
    # CENTRAL_DECISION:HajjMahmoud,MahmoudOpenLedger
    # CRISIS_EVIDENCE:1
    # NPC_TRUST:LandlordHajjMahmoud,1
    # FLAG:central_mahmoud_transaction_seen
    -> DONE
*   [Read it privately with him]
    The work is quicker. The residents receive a conclusion instead of the uncertainty.
    # CENTRAL_DECISION:HajjMahmoud,MahmoudProtectReputation
    # NPC_TRUST:LandlordHajjMahmoud,1
    # FLAG:central_mahmoud_transaction_seen
    -> DONE

=== central_mahmoud_vulnerability ===
Mahmoud admits the roof batteries are partly held together by a favor he cannot repay. His authority has been built on not showing the seams.
*   [Let the building know about the favor]
    The confession costs him status and gives the residents a chance to help repair the obligation.
    # CENTRAL_DECISION:HajjMahmoud,MahmoudOpenLedger
    # CRISIS_RESOURCES:2
    # NPC_TRUST:LandlordHajjMahmoud,2
    # FLAG:central_mahmoud_vulnerability_seen
    -> DONE
*   [Keep the favor out of the ledger]
    You protect his standing and preserve a debt nobody can collectively negotiate.
    # CENTRAL_DECISION:HajjMahmoud,MahmoudProtectReputation
    # STRESS:1
    # FLAG:central_mahmoud_vulnerability_seen
    -> DONE

=== central_mahmoud_conflict ===
Mahmoud says public accounting has turned every repair into an argument. You say private authority turned every argument into a rumor.
*   [Keep the meeting open]
    He calls you difficult, then brings another chair to the roof.
    # CENTRAL_DECISION:HajjMahmoud,MahmoudOpenLedger
    # NPC_TRUST:LandlordHajjMahmoud,-1
    # CRISIS_EVIDENCE:1
    # FLAG:central_mahmoud_conflict_seen
    -> DONE
*   [Close the meeting and accept his rule]
    The argument ends. Nobody mistakes that for agreement.
    # CENTRAL_DECISION:HajjMahmoud,MahmoudProtectReputation
    # NPC_TRUST:LandlordHajjMahmoud,1
    # FLAG:central_mahmoud_conflict_seen
    -> DONE

=== central_mahmoud_outcome ===
Mahmoud places the ledger on the roof and does not touch it. Governance has become a thing he must permit himself to share.
*   [Invite residents to sign the final account]
    The signatures are messy, slow, and more durable than a green portal checkmark.
    # CENTRAL_DECISION:HajjMahmoud,MahmoudOpenLedger
    # CRISIS_EVIDENCE:1
    # NPC_TRUST:LandlordHajjMahmoud,2
    # FLAG:central_mahmoud_outcome_seen
    -> DONE
*   [Return the ledger to his office]
    The account remains orderly. So does the silence around it.
    # CENTRAL_DECISION:HajjMahmoud,MahmoudProtectReputation
    # FLAG:central_mahmoud_outcome_seen
    -> DONE

=== central_ummkarim_transaction ===
Umm Karim asks you to carry a warning across two districts without mentioning her name. The task is small enough to look harmless and specific enough to make it risky.
*   [Carry the warning exactly]
    You pass it through people who already know how to keep a sentence alive without giving it an owner.
    # CENTRAL_DECISION:UmmKarim,UmmKarimShareWarning
    # NPC_TRUST:FixerUmmKarim,2
    # CRISIS_EVIDENCE:1
    # FLAG:central_ummkarim_transaction_seen
    -> DONE
*   [Ask her to put it in writing]
    She refuses. A written warning is easier to forward and easier to use against the person who wrote it.
    # CENTRAL_DECISION:UmmKarim,UmmKarimLimitExposure
    # NPC_TRUST:FixerUmmKarim,-1
    # FLAG:central_ummkarim_transaction_seen
    -> DONE

=== central_ummkarim_vulnerability ===
Umm Karim tells you she once gave a warning too late because she was protecting the source. She has not forgiven herself for choosing secrecy over speed.
*   [Share the source's risk with the people affected]
    You make the warning less elegant and more actionable.
    # CENTRAL_DECISION:UmmKarim,UmmKarimShareWarning
    # POLICE:1
    # NPC_TRUST:FixerUmmKarim,1
    # FLAG:central_ummkarim_vulnerability_seen
    -> DONE
*   [Protect the source and accept slower action]
    She understands the boundary because it is one she has lived with.
    # CENTRAL_DECISION:UmmKarim,UmmKarimLimitExposure
    # STRESS:1
    # FLAG:central_ummkarim_vulnerability_seen
    -> DONE

=== central_ummkarim_conflict ===
Umm Karim says your public crisis work is making the street legible to institutions that do not deserve a map.
*   [Show her the evidence before publishing]
    She removes three details and leaves the rest. Disagreement becomes editing rather than a broken alliance.
    # CENTRAL_DECISION:UmmKarim,UmmKarimShareWarning
    # CRISIS_EVIDENCE:1
    # NPC_TRUST:FixerUmmKarim,2
    # FLAG:central_ummkarim_conflict_seen
    -> DONE
*   [Publish without her review]
    The appeal gains speed and loses the protection of her network.
    # CENTRAL_DECISION:UmmKarim,UmmKarimLimitExposure
    # POLICE:3
    # NPC_TRUST:FixerUmmKarim,-3
    # FLAG:central_ummkarim_conflict_seen
    -> DONE

=== central_ummkarim_outcome ===
Umm Karim offers no blessing. She asks what kind of warning the neighborhood should remember after this month.
*   [Leave a shared warning protocol]
    The protocol belongs to no single fixer. That is its strength and its vulnerability.
    # CENTRAL_DECISION:UmmKarim,UmmKarimShareWarning
    # CRISIS_RESOURCES:1
    # NPC_TRUST:FixerUmmKarim,2
    # FLAG:central_ummkarim_outcome_seen
    -> DONE
*   [Keep the network informal]
    The network remains difficult to map and difficult for new people to enter.
    # CENTRAL_DECISION:UmmKarim,UmmKarimLimitExposure
    # NPC_TRUST:FixerUmmKarim,1
    # FLAG:central_ummkarim_outcome_seen
    -> DONE
