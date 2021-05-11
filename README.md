# Primitive Survival

<h2>Primitive Survival mod source code for Vintage Story</h2>

Documentation and latest download moved here:

https://www.vintagestory.at/forums/topic/2399-primitive-survival-traps-fishing-and-other-surprises/


Roadmap

 - Smoking Rack or Pit Smoker - smoked meat good, jerky strips less good @Boomer Bill
 - re-enable slippery fish?
 - Fish not updating in a timely fashion when picked up (i.e. apparent duplication?) - Thanks @boten
 - New: Added blood block/portion - creative only item for now 
 
 
Version 2.5.7 Updates

- New: Common configuration file ..\VintagestoryData\ModConfig\primitivesurvival.json to configure raft speed, prevent altars from dropping gold, customize traps and fishing - thanks @techrabbit, @Quixjote
- New: Polish translation courtesy of @qexow!
- New: Some code refactoring
- New: Right click pick up raft - thanks @PapaCheddar
- New: fish fillets as bait for traps and fishing - thanks Ketrai
- Fixed: Bug in German language file - thanks @Kiava
- Fixed: Raft not always reverting to default orientation (raft-north) when you break it - Thanks @Gox
- Fixed: fishing hooks, lures, nails, spikes dont smelt back into the original materials but some sub labeled material... (and wont stack with actual metal) - thanks @TechRabbit
- Fixed: changed POI on pit traps like I did to snares/deadfalls to eliminate the farm life conflict - thanks Thranos!
- Fixed: small bug in handbook guide - thanks @qexow!
- Fixed: Reworked lantern recipe so handbook links would function correctly
- Tweak: Slowed down the raft.
- Tweak: Different fish stack sizes - catfish = 16, bass,bluegill,mutated = 64, the rest = 32. Thanks @l33tmaan
- Tweak: Double the monkey bridge recipe output (yet again) - thanks @Quixote
- Tweak: Reworked fillets/fish jerky because fillet satiety in meals too low, fish jerky satiety too low - thanks @l33tmaan, others
- Tweak: - Changed mold decriptions to like Raw blah blah instead of blah blah (Raw) as per game molds


Version 2.5.6 Updates

- New: Raft - 9 logs and 4 cordage
- Fixed: Wooden lanterns recipe now showing up in the handbook

Version 2.5.5 updates

 - New: Wooden Lanterns - A simpler (primarily for aesthetics) lantern made from various wood types (requires a copper plate but no glass) - throws more light than a torch but less than a copper lantern. thanks @Kai Effelsberg
 - Updated: German translation - thanks @Kiava
 - Updated: New textures for cordage, fish fillets, and fish jerky - thanks @Ledyanaya Sonya
 - Tweak: You can now right click OR shift right click to finalize weir trap, or reset a deadfall/snare (since shift right click conflicts with Carry Capacity mod)
 - Fixed, maybe. probably: Disappearing fish, invisible fish, fish just behaving badly in general - Thanks @Willrun4fun @Tech_Rabbit @Papa Cheddar @Richard Adamsand others
 - Fixed, maybe. probably: Mod conflict with Farm Life and traps hanging game - thanks @KarsT@Lich and others
 - Fixed, maybe. probably: metal dupe issues related to molds and xSkills/XLib mod - lure mold now requires 100 units, bed-o-nails now requires 200 units - thanks @unjoyer@Tech_Rabbit  and others

Version 2.5.4 updates

 - New: updated Russian translation - thanks @Zigthehedge
 - New: German translation - thanks @Kiava
 - New: Attach pelts to vertical surfaces (as animal heads) @Vallen
 - New: Fish jerky and new grid recipe (1 knife, 4 fillets,1 salt) @l33tmaan
 - New: melt down lures and metal fishing hooks in a crucible
 - New: Soup and stew recipes added for all raw jerky types and mutated fish
 - Tweak: Fish satiety and fish fillet modifications - See the newly added tables in the documentation for details - thanks @Boomer Bill
 - Tweak: All jerky stack sizes increased to 256
 - Tweak: Made monkey bridge less expensive to craft (thanks @JakeCool19 for mentioning that on Discord)
 - Tweak: can now pick up worms with right click OR shift right click (shift right click conflicts with Carry Capacity mod) - thanks @Amenophiz
 - Fixed: Added missing textures in pot/bowl for Perch and Carp - thanks @samkee00
 - Fixed: metal dupe exploit melting down nails and spikes - thanks @Tels, @Shibby
 - Fixed: adding an oddball item (like a mechanical part) to an altar crashes the game - thanks @Hexedian
 - Fixed: nail and spike placement makes block face invisible glitch - thanks @Tels
 - Fixed: link to stake broken in guide - thanks Quixjote
 - Fixed: The male ram rug model was showing the female ram horns at the same time - thanks @lich
 - Known Issue/did not resolve: weir trap, fish baskets, limb/trotlines lose functionality after serious crash - thanks @TechRabbit 
 - Known Issue/did not resolve: sometimes removing fish from trap causes the fish to flicker/fall repeatedly for several seconds
 - Could not recreate: investigate this mod and medieval expansion not playing nice together @Kai
 - Could not recreate: Tin bronze/copper in a lure mold just resets and adds metal to the crucible? Sounds like server lag @Nozarati, @TechRabbit
 - Could not recreate: disappearing fish.  They will pop off the hook then just go poof before I can pick them up.  Maybe ice related? - thanks @willrun4fun


Version 2.5.3 updates

 - added: Two new fish - Perch and Carp
 - fixed: Orientation of the nail and spike molds in the mold rack - thanks @Jelani
 - fixed: Worm castings - thanks @Jelani
 - fixed: Worms causing server lag
 
Version 2.5.2 updates

 - added: Complete support for internationalization
 - added: More Handbook Guide information
 - added: More Russian translations
 
Version 2.5.1 updates

 - fixed: Re-enabled placement of hides on ALL surfaces
 
Version 2.5 updates

 - added: Earthworms.
 - added: Russian translation, courtesy of zipthehedge.
 - fixed: Added a patch to re-enable the creation of small pelts.  Didn't realize this got disabled in Vanilla recently.


Version 2.4 updates

 - New: fish fillet functionality for better inventory management/cooking - new item, new recipe, added to soup/stew recipes
 - New: basic taxidermy - place pelts on the floor for rugs.  Made them a little derpy to reduce z-fighting issues with large ones.  Might need a better long term solution.
 - Updated: jerky, mushrooms, bread, poultry, pickled vegetables, redmeat, bushmeat, and cheese to accepted bait types for snares, deadfalls, trot lines, limblines, and fish baskets
 - Updated: 3rd person handheld fish so they're more like holding a lantern than a club.  Changed 1st person to match somewhat.
 - Updated: More frequently removed rotten fish after a certain amount of time - they tend to pile up, especially on multiplayer.
 - Updated: Investigated fishing in general and made some minor changes to catch percents
 - Updated: Made deadfall and snare trap slightly more effective to hopefully scare off larger animals
 - Fixed: some minor z-fighting issues with fish.
 - Fixed: (More than likely) intermittent weir trap crash - prevented collisions from unsetting trap AND prevented the sneak-click from recreating trap if it was already a weir trap.
 - Fixed: Removed giant weird shadow from deadfall and fishbasket on land
 - Fixed fish in soup/stew recipes now rendering properly in pots and bowls
 - Fixed: Removed shapeless from monkey bridge grid recipe so it would pull items from the correct slots
 - Verified: Logic around relics in fish traps (i.e. gears) - seems to be aok
 - Verified: ozBillo's bushmeat mod together with this mod - seems good!

Version 2.3 updates

 - Added - metal buckets, along with smithing recipes for handles and recipes for the buckets themselves.
 - Fixed monkey bridge break/drops issues.
 - Fixed sounds for most everything.
 - Fixed bug that was allowing stakes to be replaced with other blocks.
 - Fixed steatite stair placement.

Version 2.2 updates

- moved game: assets (clayforming, knapping, and soup/stew recipes) to the primitivesurvival domain
- made fishing lure mold and fighing hook mold "rackable"
- added monkey bridge and recipe
- added metal bucket
- added spike and nail mold 
- fixed RC8 fishing crash (related to meteoric iron)

