#Issues

* Tree shapes
  * Disallow world coord too much down growth
  * Some tris go the wrong way at times.
    * Could be due to verts coming in wrong order along long side -> make verts use remaining distance
  * Leaves are a bit booring
    * Make size overscale by height in local y from root base
    * Make overscale factor determine number of verts along each side and buldge out to approximate a cirlce.

* Make generation faster
  * Allow non-interactables belong to same mesh to cut instanciation of MonoBehaviours drastically
  * Make trees have a max continuing segments and then too balance depth vs breadth

* There are no insects in the trees
  * Generation of body
  * Behaviour
  
* There are no humans (well, it is a problem from the gaming perspective)
  * Make human character
  * Tweak human behaviour
  
* The light should reflect day-cycle which should reflect level time progression
  * Add color lerp during day
  * Add night light

* Start and goal platforms
  * What parameters to use for difficulty except number of segments on level
  * Rather than restarting scene, make transition smooth...
  * Platforms are boring
    * Minimum need: place to sleep
    
* Narrative
