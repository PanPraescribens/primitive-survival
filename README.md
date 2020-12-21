# Primitive Survival

<h2>Primitive Survival mod source code for Vintage Story</h2>

Documentation and latest download moved here:

https://www.vintagestory.at/forums/topic/2399-primitive-survival-traps-fishing-and-other-surprises/


Roadmap

 - Make deadfall and snare trap more effective?, and/or prevent larger mobs from being attracted to them/tripping them.  Investigate blunt attack in 3rd person for snare.
 - Work on smoking rack
 - Pan dirt for worms, higher soil fertility, better results
 - Taxidermy - fix some faces, angle legs down slighty to decrease z-fighting, add game hooks
 - Is grid recipe for the monkey bridge not pulling from the correct slots?
 
 
Version 2.4 updates

 - Added fillet fish functionality for better inventory management/cooking
 - Changed 3rd person handheld fish so they're more like holding a lantern than a club.  Changed 1st person to match somewhat.
 - Fixed some minor z-fighting issues with fish.
 - More than likely fixed intermittent weir trap crash - prevented collisions from unsetting trap AND prevented the sneak-click from recreating trap if it was already a weir trap.
- Added jerky, mushrooms, bread, poultry, pickled vegetables, redmeat, bushmeat, and cheese to accepted bait types for snares, deadfalls, trot lines, limblines, and fish baskets
 - More frequently removed rotten fish after a certain amount of time (they were already being removed from other fishing traps) - they tend to pile up, especially on multiplayer 
 - Double checked logic around relics in fish traps (i.e. gears) - seems to be aok
 - Investigated fishing in general and made some minor changes to catch percents
 - Removed giant weird shadow from deadfall and fishbasket on land
 - Fish in soup/stew recipes now rendering properly

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

