# Seasonal and holiday narrative scenes

=== event_ramadan_start ===
The cannon sounds from the direction of the Citadel, and the city shifts. Streets that were loud go quiet. Tables appear in the lane: communal iftar spreads set by people who have nothing but still set a place for strangers. The call to prayer carries over rooftops in a voice that makes even the traffic pause.

Your mother asks if you will fast this year. The question is not really about fasting.

*   [Commit to fasting]
    You tell her yes, and something in her face softens. The discipline will cost you energy, and the hunger will sharpen your nerves, but the act of joining — of belonging to the rhythm of a city that prays together — has its own sustenance.
    # FLAG:ramadan_fasting
    # STRESS:3
    # MESSAGE:Ramadan begins. You fast with the city, and the belonging costs as much as it gives.
    -> DONE

*   [Decline to fast]
    You tell her you cannot afford the weakness this year. She nods, but the nod carries a distance, a small gap opened by a choice she will not argue with but also will not forget. In Cairo, Ramadan is not only faith. It is also identity.
    # STRESS:2
    # MESSAGE:Ramadan begins. You do not fast, and the city notices in small, accumulating ways.
    -> DONE

=== event_ramadan_iftar ===
The iftar table stretches across the rooftop, filled with dates, soup, and the particular generosity that Cairo saves for the hour when everyone is hungry at the same time. Children race up the stairs carrying plates. Old men sit cross-legged and reach for the water first, because thirst is always worse than hunger.

Your mother sits among the women and for the first time in weeks does not look like she is carrying something invisible on her shoulders.

*   [Join the communal iftar]
    You sit down among strangers and neighbors and break fast with dates that taste like forgiveness. The soup is thin but warm, the bread is shared, and for the duration of the meal, nobody asks about rent, or money, or where you have been after dark.
    # MONEY:-5
    # STRESS:-8
    # NPC_TRUST:NeighborMona,2
    # NPC_TRUST:LandlordHajjMahmoud,1
    # MESSAGE:The communal iftar feeds something that food alone cannot reach.
    -> DONE

*   [Take food home to eat quietly with your mother]
    You fill a plate from the communal spread — it is there for exactly this purpose — and carry it down the stairs. Your mother eats slowly, savoring each bite with the deliberation of someone who knows exactly what hunger costs. You sit together in the dim flat and listen to the city pray.
    # MONEY:-3
    # STRESS:-5
    # MOTHER_HEALTH:2
    # MESSAGE:You bring iftar home. The quiet meal with your mother is its own kind of prayer.
    -> DONE

=== event_ramadan_iftar_sudanese ===
The Sudanese families host their own iftar in the largest apartment on the third floor. The food is different: kisra bread, fasooliya, and a tea that tastes like the one your grandmother made in Khartoum before the planes came. The Arabic around the table shifts register, picking up the cadences of Nubi and the rhythms of a country that no longer exists the way they remember it.

Your mother closes her eyes when she tastes the tea. For the span of one meal, she is not displaced. She is just a woman eating among her people.
# STRESS:-6
# MOTHER_HEALTH:3
# NPC_TRUST:PharmacistMariam,2
# MESSAGE:The Sudanese community iftar tastes like home that no longer exists.

=== event_eid_al_fitr ===
Ramadan ends and the city explodes into celebration. Children in new clothes run through alleys that were quiet yesterday. Lanterns hang from windows. The bakery line wraps around the block twice because today is the day that even poor families find money for sweets.

The celebration demands participation. The neighbors are buying gifts. The building is exchanging plates of kahk. Your wallet is thin and your obligations are not.

*   [Buy modest gifts and join the celebration]
    You spend more than you should on a box of kahk and a new headscarf for your mother. She unwraps it with the careful delight of a woman who has learned to find joy in small things, and she wears it the rest of the day like proof that the world can still offer something beautiful.
    # MONEY:-30
    # STRESS:-6
    # NPC_TRUST:NeighborMona,2
    # MOTHER_HEALTH:3
    # MESSAGE:Eid al-Fitr. You spend beyond your means and your mother wears the proof.
    -> DONE

*   [Attend but explain you cannot afford gifts this year]
    You show up empty-handed and say so plainly. The women at the gathering nod without judgment and press sweets into your hands anyway, because Eid generosity has its own economy that does not require matching funds. The shame stings, but the warmth does too.
    # STRESS:5
    # STRESS:-3
    # NPC_TRUST:NeighborMona,1
    # MESSAGE:Eid finds you broke. The community feeds you anyway, and the gratitude has edges.
    -> DONE

*   [Stay home with your mother]
    You close the door on the noise and sit with your mother in the quiet flat. She says she does not mind. She says Eid is about God, not gifts. You both know this is true and also not enough, but the afternoon passes peacefully enough, and the sounds of celebration filter through the walls like a promise made to someone else.
    # STRESS:5
    # NPC_TRUST:NeighborMona,-3
    # MESSAGE:You skip Eid celebrations. The building notices, and the silence at home is heavy.
    -> DONE

=== event_eid_al_adha ===
The butcher's cart appears in the lane before dawn. By mid-morning, the smell of meat fills the building for the first time in weeks. The sacrifice is real — a family upstairs pooled money for a sheep, and the division of meat follows ancient rules: one third for the family, one third for friends, one third for the poor.

A plate of fresh meat arrives at your door with no name attached.

*   [Accept gratefully and cook a proper meal]
    You cook the meat with onions and cumin over a low flame while your mother watches from her mattress with an expression that wavers between gratitude and grief. She says it tastes like the Eid meals her mother used to make, before the city narrowed everything down to rent and survival.
    # FOOD:4
    # STRESS:-5
    # MOTHER_HEALTH:5
    # MESSAGE:Eid al-Adha brings meat to your door. Your mother eats well for the first time in weeks.
    -> DONE

*   [Share half with a neighbor who has even less]
    You take half the meat downstairs to the widow on the ground floor who has not been mentioned in any conversation for three days, which in Cairo means things have gotten bad enough for silence. She takes the plate and presses it against her chest and does not speak, which is how the deepest gratitude sounds in this building.
    # FOOD:2
    # STRESS:-3
    # NPC_TRUST:NeighborMona,3
    # MESSAGE:You share the Eid meat. The widow downstairs will remember.
    -> DONE

=== event_coptic_christmas ===
January in Cairo brings a different kind of quiet. The Coptic families in the building prepare for Christmas with the particular intensity of people celebrating in a country where their holiday is not the default. The smells from their kitchens — fatta, kahk, spiced tea — drift through the stairwell and make everyone's evening richer whether they celebrate or not.

Hajj Mahmoud nods politely at the Abdo family on the third floor and receives a plate of cookies in return. This is how Cairo handles difference: not by ignoring it, but by feeding it.

*   [Accept the Abdo family's invitation to share their meal]
    You sit in their apartment around a table that is too small for the number of dishes on it. Mrs. Abdo explains each dish with the pride of a woman feeding her heritage into the mouths of neighbors. The food is generous, the company warm, and the cross on the wall watches over the meal with the same quiet authority as the prayer beads in Hajj Mahmoud's hand.
    # STRESS:-5
    # FOOD:3
    # NPC_TRUST:LandlordHajjMahmoud,2
    # MESSAGE:Coptic Christmas at the Abdo family's table. Cairo feeds its own across every line.
    -> DONE

*   [Bake extra bread for the forn's holiday rush]
    The forn needs help. Christmas demand pushes the ovens past capacity, and Abu Samir sends word that extra hands mean extra pay. You work the morning shift shaping dough while the Christians worship and the Muslims sleep in, and the bakery fills with the smell of bread that will become someone else's celebration.
    # MONEY:20
    # ENERGY:-8
    # MESSAGE:You work the Christmas bakery rush. The pay is good and the forn smells like every holiday at once.
    -> DONE

=== event_sham_el_nessim ===
Spring arrives with a holiday older than any religion in the city. The streets fill with families carrying coloured eggs, salted fish, and the particular energy of people who have survived another winter. The Nile corniche turns into a promenade. Children chase each other with paint-stained fingers.

The air actually smells clean. Cairo manages this exactly once a year, and the city knows it.

*   [Join the crowds at the Nile]
    You take your mother to the corniche on a microbus that costs more than it should and moves slower than walking. But the river is wide and the breeze is real, and for two hours Cairo remembers that it was built beside something beautiful. Your mother eats feseekh for the first time in years and says the salt reminds her of being young.
    # MONEY:-10
    # STRESS:-8
    # MOTHER_HEALTH:3
    # MESSAGE:Sham el-Nessim by the Nile. The city remembers how to breathe.
    -> DONE

*   [Stay in the neighborhood for the local celebration]
    Imbaba has its own Sham el-Nessim: a street market that spills into the lane, children painting eggs on doorsteps, and a woman from the third floor who hands out coloured onions because her family has done it every spring for forty years. The celebration is smaller than the corniche but closer, and closeness has its own kind of beauty.
    # STRESS:-5
    # NPC_TRUST:NeighborMona,2
    # MESSAGE:Sham el-Nessim in Imbaba. The neighborhood celebration is small and warm.
    -> DONE

=== event_summer_solstice ===
The longest day of the year arrives with a sun that refuses to set. By six in the evening it is still forty degrees. By eight it is still thirty-five. The city moves in slow motion, as if everyone is underwater and the water is warm.

Your mother's medication needs refrigeration. The electricity has been unreliable for three days.

*   [Buy ice and keep the medication cold]
    You walk six blocks to the ice seller and carry a bag that melts faster than you can walk. The medication stays cool. Your arms ache from the weight. Summer in Cairo is a test of how much suffering you can prevent and how much you simply have to endure.
    # MONEY:-8
    # ENERGY:-5
    # STRESS:3
    # MESSAGE:You keep the medication cold with ice and sweat. Summer wins anyway, but your mother's pills survive.
    -> DONE

*   [Ask Salma to store it at the clinic]
    You walk to the clinic and ask Nurse Salma if the fridge in the back has room. She opens it without a word, moves someone's lunch to the side, and slides the medication in. She does not ask about repayment because she already knows the debt is accumulating in a currency neither of you counts out loud.
    # STRESS:-1
    # NPC_TRUST:NurseSalma,3
    # DEBT:NurseSalma,true
    # MESSAGE:Salma stores your mother's medication in the clinic fridge. The favor is added to a growing ledger.
    -> DONE

=== event_autumn_first ===
October arrives and the city exclaims. Not with words, but with windows that open for the first time in months, with children who run faster because the air no longer presses down on their chests, with the market stalls that suddenly carry fresh dates and pomegranates.

The flat is still small. The rent is still due. Your mother still coughs. But the light comes through the window at a lower angle and paints the walls gold, and for one morning, the colour of everything is enough.
# STRESS:-4
# MESSAGE:Autumn arrives. Cairo stops trying to kill you with weather, at least for a while.
-> DONE

=== event_winter_first_rain ===
The first rain of winter catches the city unprepared, as it does every year. The streets flood within minutes because the drainage system is theoretical. Drivers curse. Pedestrians scatter. A child stands in the middle of the lane with arms outstretched, tasting rain that he will remember as a miracle when he is old enough to call it weather.

Your mother opens the window and lets the smell in. Rain on dust, rain on concrete, rain on a city that was not built for rain but somehow still welcomes it.
# STRESS:-3
# ENERGY:3
# MESSAGE:The first winter rain. Cairo floods, and the child in the lane is right to be delighted.
-> DONE

=== event_spring_khamsin_warning ===
The sky turns yellow at the edges. The old women in the building start sealing windows before the radio announces anything. They know. They have known since before there was a radio, since before there was electricity, since before Cairo was tall enough to block the view of the horizon.

Someone says the khamsin will arrive by afternoon. You have until then.
# STRESS:3
# MESSAGE:A khamsin is coming. The building prepares with the urgency of people who have seen what sand can do.
-> DONE
