---
name: Sóc Nâu Đồng Hành
colors:
  surface: '#fbf9f1'
  surface-dim: '#dcdad2'
  surface-bright: '#fbf9f1'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f6f4ec'
  surface-container: '#f0eee6'
  surface-container-high: '#eae8e0'
  surface-container-highest: '#e4e3db'
  on-surface: '#1b1c17'
  on-surface-variant: '#564338'
  inverse-surface: '#30312c'
  inverse-on-surface: '#f3f1e9'
  outline: '#897266'
  outline-variant: '#ddc1b3'
  surface-tint: '#9b4500'
  primary: '#9b4500'
  on-primary: '#ffffff'
  primary-container: '#ff8c42'
  on-primary-container: '#6a2d00'
  inverse-primary: '#ffb68d'
  secondary: '#006c4f'
  on-secondary: '#ffffff'
  secondary-container: '#51fac1'
  on-secondary-container: '#007152'
  tertiary: '#006686'
  on-tertiary: '#ffffff'
  tertiary-container: '#53b5df'
  on-tertiary-container: '#00445a'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#ffdbc9'
  primary-fixed-dim: '#ffb68d'
  on-primary-fixed: '#331200'
  on-primary-fixed-variant: '#763300'
  secondary-fixed: '#55fdc4'
  secondary-fixed-dim: '#29e0a9'
  on-secondary-fixed: '#002116'
  on-secondary-fixed-variant: '#00513b'
  tertiary-fixed: '#bfe8ff'
  tertiary-fixed-dim: '#73d2fd'
  on-tertiary-fixed: '#001f2b'
  on-tertiary-fixed-variant: '#004d65'
  background: '#fbf9f1'
  on-background: '#1b1c17'
  surface-variant: '#e4e3db'
  warm-yellow: '#ffd54f'
  faded-path: rgba(0, 0, 0, 0.1)
  success-glow: '#27e0a9'
  squirrel-brown: '#897266'
typography:
  display-hero:
    fontFamily: Plus Jakarta Sans
    fontSize: 80px
    fontWeight: '800'
    lineHeight: 96px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 48px
    fontWeight: '700'
    lineHeight: 56px
  headline-lg-mobile:
    fontFamily: Plus Jakarta Sans
    fontSize: 32px
    fontWeight: '700'
    lineHeight: 40px
  instruction-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 36px
  label-caps:
    fontFamily: Plus Jakarta Sans
    fontSize: 18px
    fontWeight: '700'
    lineHeight: 24px
    letterSpacing: 0.05em
  path-number:
    fontFamily: Plus Jakarta Sans
    fontSize: 16px
    fontWeight: '800'
    lineHeight: 16px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  touch-target: 64px
  gutter: 24px
  safe-margin: 32px
  tolerance-zone: 40px
---

## Brand & Style

The design system is centered on a **Friendly, Tactile, and Encouraging** aesthetic tailored for 5-year-old children. The brand personality is anchored by "Sóc Nâu" (the Brown Squirrel), acting as a digital companion that guides rather than instructs. 

The design style is a blend of **Minimalism and Tactile/Skeuomorphism**. By using "claymorphic" elements—soft, extruded buttons with physical depth—the UI provides intuitive affordances for children with developing motor skills. The visual language avoids the "fear of failure" by replacing harsh error states with gentle guidance, glowing paths, and celebratory rewards. The overall mood is optimistic, safe, and focused on the "joy of discovery."

## Colors

The palette uses a **Warm Cream (#fbf9f1)** base instead of pure white to reduce eye strain and create a paper-like, approachable canvas. 

- **Primary (#ff8c42):** The "Sóc Nâu" signature orange, used for primary interactive elements and the character's brand presence.
- **Secondary (#51fac1):** A vibrant but soft green used exclusively for positive feedback, success states, and completed paths.
- **Tertiary (#53b5df):** A calm sky blue for instructional guidance, information boxes, and secondary actions.
- **Guidance Logic:** Exercises use "Faded" (translucent) paths for independent practice and "Glowing" (high-saturation secondary) paths when providing active correction. Large red "X" marks are strictly forbidden; error prevention is handled through subtle color shifts and encouraging audio cues.

## Typography

Typography prioritizes formal correctness and legibility over decoration. **Plus Jakarta Sans** is the sole typeface, chosen for its open apertures and friendly, rounded terminals which mirror the shape language of the UI.

- **Learning Letters/Numbers:** Displayed at the `display-hero` size (80px+) in the center of the drawing area.
- **Clarity for Vietnamese:** Font weights and line heights are adjusted to ensure that Vietnamese diacritics (marks) are clearly separated and highly visible for children learning to read.
- **Stroke Order:** Small, high-contrast circular labels use `path-number` to guide the sequence of drawing (e.g., "1", "2", "3") placed precisely at start points.
- **No Decorative Fonts:** To prevent cognitive confusion, stylized or script fonts are excluded from instructional areas.

## Layout & Spacing

The layout follows a **One Task Per Screen** philosophy to minimize cognitive load. Content is centered, with the "Drawing Area" or "Interaction Zone" occupying at least 80% of the viewport.

- **Safe Zones:** A 32px margin is maintained around all edges to prevent accidental triggers near device bezels.
- **Touch Targets:** A minimum 64px hit area is required for all buttons to accommodate the motor skills of 5-year-olds.
- **Tolerance Zones:** Tracing and drag-and-drop tasks include an invisible 40px "tolerance buffer" around paths, allowing children to be slightly imprecise without failing the task.
- **Responsive Reflow:** On mobile, controls (Like "Tiếp tục") are pinned to a bottom action bar, while on tablets, they are positioned on the side to allow for comfortable two-handed use.

## Elevation & Depth

Visual hierarchy is established through **Physicality and Light**.

- **Claymorphism:** Buttons use a solid, darker 4px-6px bottom offset (fake 3D) rather than a soft shadow. This makes the buttons look like physical blocks that can be pushed down.
- **Active Glow:** When a task is active or a correct path is suggested, the element utilizes a "Success Glow"—an outer ambient shadow tinted with the secondary color.
- **Target Zones:** In drag-and-drop tasks, "Drop Zones" are indicated by an inner shadow (inset), making the area look like a physical hollow meant to be filled.
- **Adult Zone:** PIN-protected overlays use a high-density backdrop blur (20px) to clearly separate the parental controls from the child's learning environment.

## Shapes

The shape language is consistently **Rounded (Level 2)** to maintain a safe, soft feel.

- **Interactive Elements:** Buttons and selection chips use a minimum 16px (1rem) radius.
- **Tracing Points:** Start points are always perfect circles (Chấm màu) to indicate a "homing" spot for the finger.
- **Path Indicators:** Directional arrows have rounded heads and tails to avoid sharp points.
- **Mathematical Shapes:** Geometric shapes (squares, triangles) used in math exercises should have slightly softened corners (2px-4px radius) to fit the overall friendly aesthetic without losing their mathematical identity.

## Components

- **Tracing (Vẽ theo nét):** Includes a "Start Dot" (Primary Orange), "Directional Arrow," and three path states: Bold (Guided), Dashed (Practice), and Faded (Independent).
- **Selection (Chọn đáp án):** Large cards that "pop" (scale up 105%) when tapped. Correct answers trigger a Sóc Nâu "Joy" animation and turn the card border to Secondary Green.
- **Drag-and-drop (Kéo-thả):** Objects must "stick" to the finger with a slight offset so the child can see what they are moving. Target zones provide a "magnetic" snap effect when the object is within the tolerance zone.
- **Mathematical Elements:** Numbers are rendered in `display-hero` size. Counting objects (e.g., apples, stars) must move or pulse slightly when touched to provide tactile feedback for 1-to-1 correspondence.
- **The "Continue" Button:** Remains in a "Dim" state (low opacity) until task criteria are met, at which point it performs a "Breath" animation and transitions to full Primary Orange.
- **Sóc Nâu Feedback:** A persistent dialogue component that uses a Warm Yellow background and always includes a visible "Audio" icon to replay instructions.