Shards of a Life
================

Experimental Unity 2D generative platform game

Synopsis
--------

The futility of life expressed as a repitative fragmented daily exersise to survive until the day's end.
Each day you start kind of whole and every time you suffer the interaction of a fellow human or other wild life a piece of yourself breaks.
Each day you struggle to make it through a corrupt and strange world without completely loosing it completely and before the world completely falls apart.

Technical
---------

The ground is generated with various applications of the Perlin noise function

Buildings and Greens (trees/shrubs) share a basic generative code with differences mainly being how the children are oriented and their number. Then of course also it is a difference in each segments mesh.

All interactable features becomes fragmented using a random triangle fill algorithm explained on Wolfram|Alpha.

Status
------

* Tree generation working
* Building generation working
* Ground type segment generation working
* Segment to segment linking working
* Fragmentation of meshes working
* Basic input + physics working
* Disruptive beams
* Make complete level generation by connecting ground segment generation with object generation (buildings and trees)


ToDo
----

* Make tree generation faster
* Resolve bug that places houses in bad places
* Tweak tree parameters
* Make insect generation
* Make insect behaviour
* Make human character
* Make human behaviour
* Make day-cycle
* Make bed as level start & goal
* Make level progression
* Add narrative
