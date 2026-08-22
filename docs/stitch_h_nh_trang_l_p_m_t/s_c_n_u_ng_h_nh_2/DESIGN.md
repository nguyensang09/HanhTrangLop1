---
name: Sóc Nâu Đồng Hành
colors:
  surface: '#fbf9f1'
  surface-dim: '#dcdad2'
  surface-bright: '#fbf9f1'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f5f4ec'
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
  secondary-fixed: '#54fdc4'
  secondary-fixed-dim: '#27e0a9'
  on-secondary-fixed: '#002116'
  on-secondary-fixed-variant: '#00513b'
  tertiary-fixed: '#bfe8ff'
  tertiary-fixed-dim: '#73d2fd'
  on-tertiary-fixed: '#001f2a'
  on-tertiary-fixed-variant: '#004d65'
  background: '#fbf9f1'
  on-background: '#1b1c17'
  surface-variant: '#e4e3db'
typography:
  display-hero:
    fontFamily: Plus Jakarta Sans
    fontSize: 40px
    fontWeight: '800'
    lineHeight: 48px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 32px
    fontWeight: '700'
    lineHeight: 40px
  headline-lg-mobile:
    fontFamily: Plus Jakarta Sans
    fontSize: 24px
    fontWeight: '700'
    lineHeight: 32px
  title-md:
    fontFamily: Be Vietnam Pro
    fontSize: 22px
    fontWeight: '600'
    lineHeight: 30px
  body-lg:
    fontFamily: Be Vietnam Pro
    fontSize: 20px
    fontWeight: '500'
    lineHeight: 32px
  body-md:
    fontFamily: Be Vietnam Pro
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  label-caps:
    fontFamily: Plus Jakarta Sans
    fontSize: 16px
    fontWeight: '700'
    lineHeight: 20px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  unit: 8px
  container-margin: 24px
  gutter: 16px
  touch-target-min: 48px
  card-gap: 20px
---

## Brand & Style
The design system is centered on a "Learning through Play" philosophy, specifically tailored for preschoolers transitioning to primary school. The aesthetic is **Tactile & Soft**, utilizing a "claymorphic" influence—elements appear slightly 3D, soft, and physically pressable to encourage exploration.

The core personality is guided by **Sóc Nâu** (the Brown Squirrel), manifesting in the UI through warm tones, organic shapes, and encouraging visual feedback. The goal is to evoke a sense of safety, curiosity, and accomplishment. High-contrast outlines are avoided in favor of soft shadows and tonal shifts to keep the interface gentle on young eyes while maintaining high legibility for beginning readers.

## Colors
The palette is designed to be vibrant yet balanced.
- **Primary (Friendly Orange):** Used for main actions and Sóc Nâu’s brand presence.
- **Secondary (Soft Green):** Reserved for "Success" states, progress bars, and "Correct" feedback.
- **Tertiary (Sky Blue):** Used for informational elements and secondary navigation.
- **Background (Cream):** A #FFFDF5 base reduces the harshness of pure white, providing a paper-like warmth that is easier on the eyes during long learning sessions.
- **Accent (Warm Yellow):** Used for stars, achievements, and highlighting special tips from the character.

## Typography
Typography must prioritize legibility for children who are just learning to recognize letterforms. 
- **Headlines:** Use **Plus Jakarta Sans** for its friendly, open apertures and high x-height. It feels modern and approachable.
- **Body & Instructional Text:** Use **Be Vietnam Pro** specifically for its excellent Vietnamese diacritic placement, ensuring marks are clear and not cramped.
- **Sizing:** Minimum body size is set to 18px to accommodate tablet usage and developing motor skills. Line height is generous (1.5x+) to prevent lines of text from blurring together for early readers.

## Layout & Spacing
The layout follows a **Fluid Content Model** with a strong emphasis on "Safe Zones."
- **Grid:** A simple 4-column grid for mobile and 8-column for tablet. Content is usually centered in large, easy-to-hit containers.
- **Touch Targets:** All interactive elements must maintain a minimum 48x48px hit area, though 64px is preferred for primary navigation buttons to suit "fat-finger" interactions.
- **Spacing Rhythm:** Based on an 8px scale. Use 24px margins on all screen edges to prevent accidental triggers near the bezel.

## Elevation & Depth
This design system avoids traditional drop shadows in favor of **Tonal Offsets** and **Inner Glows**.
- **The "Pressable" Effect:** Buttons use a solid bottom-border (4px) in a darker shade of the button color to create a 3D effect. When pressed, the element translates Y+2px and the border shrinks, simulating a real physical button.
- **Cards:** Use a very soft, large-radius ambient shadow (`blur: 20px, opacity: 0.05, color: #FF8C42`) to make them feel like they are floating slightly above the cream background.
- **Modals:** Use a full-screen backdrop blur (10px) with a semi-transparent cream overlay to keep the focus entirely on the learning task.

## Shapes
The shape language is strictly **Rounded**. 
- Standard components (Inputs, Small Buttons) use a **0.5rem (8px)** radius.
- Container elements (Cards, Modals) use **1rem (16px)**.
- Featured elements (Sóc Nâu dialogue bubbles, Progress containers) use **1.5rem (24px)** or full pill-shaping. 
- Avoid any sharp 90-degree corners to maintain the "safe and friendly" psychological profile of the app.

## Components
- **Primary Action Buttons:** Large, pill-shaped, with a 3D "clay" effect. Always use high-contrast white text against the Primary Orange.
- **Instructional Cards:** White background with a 2px stroke in a lightened version of the Sky Blue. These house the "Lesson of the day" or "Practice" activities.
- **Progress Bars:** Thick (16px height) bars with a rounded track. The "fill" should be the Soft Green, and the "handle" or "end-point" should be a Small Star icon or a Sóc Nâu head icon.
- **Selection Chips:** Used for multiple-choice answers. These should change from Neutral Cream to Sky Blue when selected, with a clear "pop" animation.
- **Iconography:** Use thick-stroke (2pt) icons with rounded ends. Icons should be multi-colored or duo-tone to appear more like illustrations than functional symbols.
- **Sóc Nâu Dialogue:** A persistent component at the top or bottom of the screen where the character provides text-to-speech instructions. The background is always the Accent Warm Yellow to denote "Help."