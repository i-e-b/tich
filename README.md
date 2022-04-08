# tich
Experimental 2D graphics format &amp; renderers

## What?

This is an experiment based around using 2D signed distance fields as a vector graphics format.
There will probably be 3 parts:
 - The format, cross platform
 - A C# renderer
 - A JavaScript / HTML5 canvas renderer

## Why?

1. For fun
2. To see if I can make a vector format that is both very small and very adaptable
3. It should be easy to make a high quality renderer, with antialiasing and scale invariance for free

## Where?

`libtich` contains the initial C# core. `tich.exe` will render a .tich file to a PNG.
Javascript should come along soon if the initial experiment works ok.


# Plan

Each layer is essentially an AST for a math expression that can describe the SDF.
Have some general math plus specialised SDF stuff (like softmin/softmax)
Probably an RPN stack language? Should be possible to run many in parallel if needed,
or translate into a shader.

The language should avoid loops wherever possible, and keep logic to a total minimum

We run the distance function for a sample of points (using the distance to minimise sampling).
The program is run for each of these points, with the point being sampled as the only value in the stack.
(therefore an empty program should output a 1 pixel line down the left of the image)

## Targets

1. It should support color and transparency
2. Should be able to compose multiple 'layers'
3. Aiming mostly for small vector-icon like images. Think FontAwesome.

## Format

File format something like:

```
uint24 magic <-- format magic
uint8  version <-- version number
uint32 ARGB   <-- initial background

int32 vT,vL,vB,vR  <-- visual top, left, bottom, right (in pixels)
fix16:16 sT, sL, sB, sR  <-- the 'p' values at the TLBR point~~~~s

n*[            <-- per layer
    uint16 len   <-- size of this layer in bytes
    uint8 mode  <-- composition mode (set, add, subtract, multiply ...?)
    uint32 ARGB <-- color for this layer

    n*[         <-- drawing commands
        uint8 cmd <-- command type
        n*[ <-- count of args is fixed for each command. May be zero.
            fix16:16 arg <-- argument (meaning depends on command)
        ]
    ]
]

```

### Types?

All commands should be able to take in any of:

scalar, vec2, vec3, vec4

and can output any of those (doesn't have to be the same).

### Commands?

See the C# enum `Command` for details

#### pre-baked SDF functions (output a distance, can be modified further)

unevenCapsule/2:1, triangle/3:1, circle/2:1, ellipse/1:1,

polyAlt/n:1, polyWind/n:1 (polygons use all remaining stack)


Also bring in others from https://iquilezles.org/www/articles/distfunctions2d/distfunctions2d.htm

and

https://iquilezles.org/www/articles/distfunctions2dlinf/distfunctions2dlinf.htm