# primitive-survival
<h2>Primitive Survival Mod for Vintage Story</h2>

Here's the thing:  I'm a huge fan of Vintage Story and especially its <i>Wilderness Survival</i> mode.  I also have a keen interest in real life primitive survival. This mod is my attempt to to meld the two to provide you with a more realistic early game experience.  There's still a lot of testing and fine tuning on the docket so consider this a preview (perpetual beta might be a more accurate description).

<h3>Traps</h3>

There are two traps - the <b>Deadfall</b> and the <b>Snare</b> - and they are very similar in nature.  They are intended to kill smaller animals on your behalf.  The only real difference is that the deadfall might kill a chicken or a hare (or a baby anything), while the snare might be able to take down bigger entities (like a fox or a boar) as well.

In BOTH cases:

 - Larger entities will take minimal damage when coming into contact with one of these traps, and trip it.  
 - Once a trap's been tripped, you'll need to break it and place it down to set it again.
 - You can right click on any unbaited set trap to bait it (with fruit in hand).

Deadfall (Set): One of a number of stones, and a stick
<recipe>

Deadfall (Set and Baited): One of a number of stones, a stick, and any type of fruit
<recipe>

Deadfall (Set and Baited), shapeless: A deadfall and any type of fruit
<recipe>

Snare (Set): Twine and a stick
<recipe>

Snare (Set and Baited): Twine, a stick, and any type of fruit
<recipe>

Snare (Set and Baited), shapeless: A deadfall and any type of fruit
<recipe>


<h3>Fishing</h3>

There are currently two methods for fishing - a <b>Fish Basket</b> and a <b>Trotline</b>.  They have some similarities but your mileage may vary when it comes to the preferred method.

In BOTH cases:

 - A fish might simply steal your bait.
 - You can right click to remove fish.  Careful though, they're a little slippery sometimes!
 - You can right click (on an unbaited hook or fish basket) to re-bait it (with fruit in hand).

The fish basket is pretty strightforward.  Place it in water but make sure there's a free block of water in front of it.  Now hurry up and wait...and maybe one or two fish will find there way into your basket.
You might want to check on your fish baskets a little more frequently than a trotline, since fish can escape quite easily.

Fish Basket: Twine and four sticks
<recipe>

Fish Basket (Baited): Twine, four sticks, and any type of fruit
<recipe>

Fish Basket (Baited), shapeless: A fish basket and any type of fruit
<recipe>

The trotline's a little more complicated because You're going to need hooks.  They can be knapped with flint or one of several stones.

<knapping>

Place a trotline on a dirt block facing another equal height dirt block.  Ideally there's some water between those two dirt blocks.  To place it successfully, you'll need as many trotlines as there are water blocks between the two dirt blocks, and a clear line of sight.
Once your trotline is in place, you'll need to attach your hooks (or baited hooks) to it by right clicking on a section of trotline (with hook in hand).  Obviously if your hook isn't in water you are less likely going to catch a fish.

Trotline: Twine and two sticks
<recipe>


What do I do with all these raw fish?  Cook 'em up on a fire and eat 'em of course.  Note: The official Vintage Story Survival Bass had some issues with it's configuration, so I "borrowed" its json file/shape/texture, and reworked it as "Big Bass" (for lack of a better name).  I've added a trout for variety, but it isn't pretty because modelling (much like coding and documenting) is NOT my forte.  

<h3>Jerky</h3>

The premise here is that you can "cure" meat to create <b>Jerky</b> simply by cutting it up into small strips and letting it dry in the sun.  So if you cut up your redmeat or bushmeat it will start to cure automatically.  Note: You can still cook it afterwards if you like.

Jerky (Redmeat) shapeless: A knife and a piece of redmeat
<recipe>

Jerky (Bushmeat) shapeless: A knife and a piece of bushmeat
<recipe>


<h3>Rope Bridge</h3>

The <b>Rope Bridge</b> is a bit of an aside from primitive survival, and it's currently the only block that's suitable for Creative Building.  Build it with three boards (any type of wood) and two trotlines.  I added this block because it wasn't much of a stretch after getting that trotline mostly fleshed out.

<Recipe>

Place it just like you would a trotline - on an elevated dirt block facing another elevated dirt block.  To place it successfully, you'll need as many rope bridges as there are empty blocks between the two elevated dirt blocks, and a clear line of sight. Whether it's over water or up in the mountains, it will get you where you need to be!


<h3>Roadmap</h3>

It's mostly going to be testing and tweaking for now, but I've got plans.  Big plans...

 - Figure out the whole server/client thing.
 - Get to the bottom of how to attract animals to my baited traps, much like the trough does.
 - Add more types of fish (and perhaps other random aquatic suprises).
 - Add additional primitive fishing options - a limb line, a funnel trap, and maybe even a primitive fishing rod.
 - Add additonal primitive traps, like figure four (or other) deadfalls, a pitch trap, etc. 
 - Vintage Story's Lovecraft inspired theme is also right up my alley as I have a real passion for pulp fiction, so I may just veer off in that direction.
