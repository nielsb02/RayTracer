-- Read Me File --

Contributers:
	-	Niels Blonk 
	-	Marc de Jong 

Project details:
	-	We have provided a few presets to browse through to show our various scenes. This is adjustable by holding one of the number keys (0-6);

	-   All the non-assigned pixels on the screen will be given a blueish color to represent a sky. 

	-	Fast approximate anti-aliasing is implemented. It is applied after all the ray computations. It can be found in the raytracer.cs
		in the AntiAliasing method. It uses the 8 pixels around a certain pixel to determine it's color. 

	-	Super sampling anti-aliasing is implemented. It can be found in the raytacer.cs file in the renderSim method. It can be switch of by setting the condition to false.
		The SSAA shoots four rays for each screen pixel and prints the average color.

	-	Triangle support is implemented. A triangle is a primitive as well and is calculated sort of as a plane with an additional check whether
		whether the point in within three points.

	-	Spotlight are implemented as well. Spotlights have two additional parameters compared to a normal light namely: direction and angle. If a light source is
		a spotlight the angle between the shadow ray and the direction of the light must be less than the angle / 2, otherwise no light is shown.

	-	Refraction is implemented on Spheres. A Sphere can even be both refractive and reflective, which are related by Fresnel's coefficient. 
		it creates a new ray when entering or leaving a refractive object, which is determined by using the refractive index of air and the refractive index of the object.
		we added the refractive index of a few materials: vacuum, air, water, ice, glass and diamond. Two of these implemented in scene 4.

	-	Textures are implemented on primitives. Now only earth, snow, lava and rock are introduced in scene 5.

	-	It is possible to move the camera with 5 degrees of freedom and to switch mode with the following
		Controls:
			W: Move forward (along Y-axis)
			S: Move Backward (along Y-axis) 
			S: Move Left (along X-axis)
			D: Move Right (along X-axis)
			Q: Move Down (along Z-axis)
			E: Move Up (along Z-axis)
			Arrow up: Tilt upward
			Arrow down: Tilt downward
			Arrow left: Turn left 
			Arrow right: Turn right

			Minus: Decrease FOV
			Plus: Increase FOV

			T: Tracker/debug mode
			R: Raytracer mode
			B: Enable/disable fast approximate anti-aliasing  

			0-6: Select preset

			--Note: controls will only detect input at the start of a new tick so it might me needed to hold the key for a longer amount of time. (sometimes up to 15 seconds)
					We recommend walking around in the debug mode since this is a bit quicker and switching back to the raytracer to see the render.

Sources:
	-	these sources were used for inspiration and code examples, however no code has been copied. 
	https://blog.demofox.org/2017/01/09/raytracing-reflection-refraction-fresnel-total-internal-reflection-and-beers-law/
	https://people.cs.clemson.edu/~dhouse/courses/405/notes/texture-maps.pdf
	https://gdbooks.gitbooks.io/3dcollisions/content/Chapter4/point_in_triangle.html
	https://www.scratchapixel.com/

