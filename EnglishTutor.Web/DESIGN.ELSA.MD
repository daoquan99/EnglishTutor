# Design System Inspired by ELSA Speak

## 1. Visual Theme & Atmosphere

ELSA Speak's design system embodies a modern, tech-forward educational platform with a sophisticated dark aesthetic paired with vibrant accent colors. The visual language combines deep navy and purple tones with bright cyan accents, creating a sense of innovation and learning energy. The system emphasizes clarity and accessibility through generous whitespace, precise typography, and thoughtful color contrast. This creates an immersive, professional environment that feels both approachable and advanced—ideal for users learning English in a digital-first world.

**Key Characteristics:**
- Deep indigo/navy foundation with purple accent hierarchy
- High contrast between dark backgrounds and bright UI elements
- Clean, modern typography with consistent spacing
- Vibrant cyan highlights for interactive and success states
- Educational yet premium aesthetic with technology-forward vibes
- Accessibility-focused with clear visual hierarchy
- Bright accent colors that stand out against dark surfaces

## 2. Color Palette & Roles

### Primary
- **Deep Navy** (`#322066`): Primary brand color used throughout UI, buttons, and backgrounds. Establishes the core dark foundation.
- **Dark Indigo** (`#171236`): Secondary dark tone for contrast and layering within dark backgrounds.
- **Midnight** (`#050024`): Darkest neutral for emphasis and depth on primary elements.

### Accent Colors
- **Primary Purple** (`#6441CC`): Main interactive purple for CTAs and highlights.
- **Vibrant Purple** (`#7D51FF`): Lighter purple accent for secondary interactions and hover states.
- **Light Lavender** (`#9774FF`): Tertiary purple for disabled or muted states.
- **Pale Lilac** (`#CBB9FF`): Very light purple for backgrounds and subtle accents.
- **Cyan Bright** (`#38ECEC`): Vibrant cyan for success states, highlights, and primary call-to-action buttons.

### Interactive
- **Cyan Primary** (`#38ECEC`): Primary interactive color for buttons, links, and active states.
- **Purple Interactive** (`#6441CC`): Alternative interactive purple for secondary actions.

### Neutral Scale
- **Pure White** (`#FFFFFF`): Primary text and content color on dark backgrounds.
- **Light Gray** (`#CCCCCC`): Secondary text and disabled states.
- **Medium Gray** (`#B3B3B3`): Tertiary text for captions and hints.
- **Pure Black** (`#000000`): Maximum contrast for emphasis (minimal use).

### Surface & Borders
- **Off-White Background** (`#F9F6FF`): Very light purple-tinted white for alternate surfaces.
- **Light Gray Surface** (`#E9E8EC`): Slightly darker light surface for layering.
- **Pale Lavender Background** (`#F2EEFF`): Ultra-light background for soft containers.
- **Purple Border** (`#322066`): Primary border color for cards and containers.

### Semantic / Status
- **Success Green** (`#38EC5F`): Success states and positive feedback.
- **Warning Yellow** (`#FFFF2C`): Warning states and alerts.
- **Warning Orange** (`#FF9822`): Secondary warning for important notices.
- **Error Red** (`#FF2F2F`): Error states and validation failures.

## 3. Typography Rules

### Font Family
**Primary:** SF Pro Display, -apple-system, BlinkMacSystemFont, system-ui, sans-serif
**Secondary:** SF Pro Display (consistent throughout; fallback to system sans-serif stack)

### Hierarchy

| Role | Font | Size | Weight | Line Height | Letter Spacing | Notes |
|------|------|------|--------|-------------|----------------|-------|
| Display / H1 | SF Pro Display | 36px | 700 | 42px | 0px | Page titles and hero headings |
| Heading H3 | SF Pro Display | 20px | 600 | 28px | 0px | Section headings |
| Heading H2 | SF Pro Display | 18px | 600 | 26px | 0px | Subsection headings |
| Heading H4 | SF Pro Display | 14px | 400 | 24px | 0px | Small headings |
| Heading H6 | SF Pro Display | 13px | 600 | 16px | 0px | Micro headings and labels |
| Body Large | SF Pro Display | 16px | 700 | 24px | 0px | Emphasized body text and button text |
| Body Regular | SF Pro Display | 16px | 400 | 24px | 0px | Primary body text and content |
| Body Small | SF Pro Display | 13px | 400 | 20px | 0px | List items and secondary text |

### Principles
- Consistent use of SF Pro Display across all text elements ensures visual coherence
- Weight progression from 400 (regular) to 700 (bold) provides clear hierarchy
- Line heights maintain generous vertical spacing for readability on dark backgrounds
- 16px serves as the baseline for body text; smaller sizes used sparingly for secondary content
- All sizes are pixel-perfect for digital display without scaling ambiguity

## 4. Component Stylings

### Buttons

**Primary Button (Large)**
- Background: `rgba(56, 236, 236, 1)` (Cyan)
- Text Color: `#000000`
- Font: SF Pro Display, 16px, weight 400
- Padding: `16px 24px`
- Border Radius: `50%` or `80px` (pill-shaped)
- Border: `0px solid transparent`
- Box Shadow: `none`
- Height: `48px` (minimum)
- Line Height: 24px
- Hover: Opacity decrease to 0.85

**Secondary Button (Medium)**
- Background: `rgba(100, 65, 204, 1)` (Purple)
- Text Color: `#FFFFFF`
- Font: SF Pro Display, 13px, weight 700
- Padding: `0px 12px`
- Border Radius: `80px`
- Border: `0px solid transparent`
- Box Shadow: `none`
- Height: `28px`
- Line Height: 16px
- Hover: Background to lighter purple `#7D51FF`

**Ghost Button**
- Background: `transparent`
- Text Color: `#FFFFFF`
- Font: SF Pro Display, 16px, weight 400
- Padding: `0px 0px`
- Border Radius: `0px`
- Border: `0px solid transparent`
- Box Shadow: `none`
- Height: auto
- Line Height: 24px
- Hover: Text color to `#38ECEC`

**Small Pill Button**
- Background: `transparent`
- Text Color: `#FFFFFF`
- Font: SF Pro Display, 13px, weight 700
- Padding: `0px 12px`
- Border Radius: `3.355e+07px` (pill)
- Border: `0px solid transparent`
- Box Shadow: `none`
- Height: `28px`
- Line Height: 16px

### Cards & Containers

**Card Base**
- Background: `rgba(0, 0, 0, 0)` or transparent
- Text Color: `#FFFFFF`
- Font: SF Pro Display, 16px, weight 400
- Padding: `16px 20px`
- Border Radius: `16px`
- Border: `1px solid #322066` (Purple border)
- Box Shadow: `none`
- Height: fit-content
- Width: auto
- Line Height: 24px

**Card Elevated (Hover State)**
- Border: `1px solid #6441CC`
- Box Shadow: `rgba(0, 0, 0, 0.1) 0px 20px 25px -5px, rgba(0, 0, 0, 0.1) 0px 8px 10px -6px`
- Transition: 0.3s ease

### Inputs & Forms

**Input Field Base**
- Background: `rgba(23, 18, 54, 1)` (Dark Indigo)
- Text Color: `#FFFFFF`
- Font: SF Pro Display, 16px, weight 400
- Padding: `12px 16px`
- Border Radius: `12px`
- Border: `1px solid #322066`
- Line Height: 24px
- Height: `48px`

**Input Focus State**
- Border: `1px solid #38ECEC` (Cyan)
- Box Shadow: `0 0 0 3px rgba(56, 236, 236, 0.1)`

**Input Error State**
- Border: `1px solid #FF2F2F` (Red)
- Box Shadow: `0 0 0 3px rgba(255, 47, 47, 0.1)`

**Input Disabled State**
- Background: `rgba(50, 32, 102, 0.5)`
- Text Color: `#B3B3B3`
- Border: `1px solid #322066`
- Cursor: not-allowed

### Navigation

**Navigation Bar**
- Background: `rgba(23, 18, 54, 1)` (Dark Indigo with opacity)
- Text Color: `#FFFFFF`
- Font: SF Pro Display, 16px, weight 400
- Padding: `16px 32px`
- Border Radius: `0px`
- Border: `0px solid transparent`
- Box Shadow: `rgba(0, 0, 0, 0.1) 0px 4px 6px -1px, rgba(0, 0, 0, 0.1) 0px 2px 4px -2px`
- Height: auto
- Width: 100%
- Line Height: 24px

**Navigation Link (Active)**
- Text Color: `#38ECEC` (Cyan)
- Font Weight: 600
- Border Bottom: `2px solid #38ECEC`

**Navigation Link (Hover)**
- Text Color: `#38ECEC`
- Transition: 0.2s ease

### Links

**Text Link**
- Background: transparent
- Text Color: `#38ECEC` (Cyan)
- Font: SF Pro Display, 16px, weight 400
- Text Decoration: none
- Hover: Text Decoration underline, Opacity 0.8

**CTA Link Button**
- Background: `rgba(100, 65, 204, 1)` (Purple)
- Text Color: `#FFFFFF`
- Font: SF Pro Display, 16px, weight 400
- Padding: `16px 16px`
- Border Radius: `12px`
- Border: `0px solid transparent`
- Height: `58px`
- Width: auto
- Line Height: 24px
- Hover: Background to `#7D51FF`

## 5. Layout Principles

### Spacing System
**Base Unit:** 4px

**Scale:** 4px, 8px, 12px, 16px, 20px, 24px, 28px, 32px, 40px, 44px, 52px, 60px

**Usage Context:**
- `4px`: Micro-adjustments, icon spacing
- `8px`: Tight component spacing, small gaps
- `12px`: Button padding, form field gaps
- `16px`: Standard padding, card content, navigation elements
- `20px`: Medium padding, section spacing
- `24px`: Large padding, component margins
- `28px-32px`: Section gap, content rhythm
- `40px-60px`: Major section breaks, hero spacing

### Grid & Container
- **Max Width:** 1440px for desktop content
- **Gutter:** 32px between columns
- **Column Strategy:** 12-column flexible grid
- **Container Padding:** 32px on desktop, 20px on tablet, 16px on mobile
- **Section Padding:** 60px vertical (top/bottom), 32px horizontal on desktop

### Whitespace Philosophy
ELSA Speak employs generous, purposeful whitespace to create visual breathing room and reduce cognitive load. Dark backgrounds are offset by clear separation between elements, with consistent gaps that scale proportionally. Whitespace reinforces hierarchy and guides user attention through intentional negative space rather than visual noise.

### Border Radius Scale
- `0px`: Navigation bars, full-width elements
- `12px`: Input fields, standard containers
- `16px`: Cards, standard card containers
- `20px`: Large containers, hero sections
- `80px`: Pill-shaped buttons, rounded CTAs
- `50%`: Perfectly circular icons and avatar containers
- `3.355e+07px`: Maximum pill effect (essentially circular)

## 6. Depth & Elevation

| Level | Treatment | Use |
|-------|-----------|-----|
| Flat (0) | No shadow, transparent background | Base cards, disabled states |
| Subtle (1) | `rgba(0, 0, 0, 0.1) 0px 4px 6px -1px, rgba(0, 0, 0, 0.1) 0px 2px 4px -2px` | Navigation bars, hover states |
| Elevated (2) | `rgba(0, 0, 0, 0.1) 0px 20px 25px -5px, rgba(0, 0, 0, 0.1) 0px 8px 10px -6px` | Modal dialogs, floating cards, important containers |
| Focus (3) | Cyan glow + elevation 2 shadow | Focused inputs, active modals |

**Shadow Philosophy:**
ELSA Speak uses minimal shadows anchored in deep blacks with 10% opacity, creating subtle depth without heaviness. Shadows serve to separate interactive elements from background layers and indicate elevation changes. The system avoids heavy, ornate shadows—instead favoring clean, precise elevation that maintains the modern aesthetic and ensures accessibility on dark backgrounds.

## 7. Do's and Don'ts

### Do
- Use cyan (`#38ECEC`) for all primary interactive elements and success states to maintain visual consistency and engagement
- Maintain minimum text contrast ratio of 4.5:1 on dark backgrounds for WCAG AA compliance
- Apply generous padding (16px minimum) within cards and containers for breathing room
- Use the purple accent hierarchy (`#6441CC`, `#7D51FF`, `#9774FF`) for secondary interactions in order of prominence
- Layer components with the consistent border radius scale (12px, 16px, 20px) for visual cohesion
- Implement smooth transitions (0.2s-0.3s ease) on all interactive elements for refined feedback
- Use SF Pro Display consistently across all text for unified typography
- Test all colors against the dark navy background (`#322066`) for sufficient contrast
- Apply 16px font size as the baseline for body text; scale up or down intentionally

### Don't
- Mix border radius values outside the defined scale (0px, 12px, 16px, 20px, 80px, 50%)
- Use gray (`#B3B3B3`) as a primary text color; reserve it only for secondary or disabled states
- Apply shadows to flat elements without elevation context
- Combine multiple accent colors in a single component; let hierarchy guide color usage
- Use light backgrounds (`#F9F6FF`, `#E9E8EC`) on the dark navy base without sufficient contrast testing
- Implement font weights outside the specified scale (400, 600, 700)
- Stack padding and margins inconsistently; always reference the spacing system
- Create interactive elements smaller than 28px height or 80px width for touch accessibility
- Mix cyan and purple accents within the same interactive element; choose one per component
- Ignore line-height specifications; maintain vertical rhythm from the hierarchy table

## 8. Responsive Behavior

### Breakpoints

| Breakpoint | Width | Key Changes |
|------------|-------|-------------|
| Mobile | 320px–767px | Single column, 16px container padding, 20px section gaps, 13px body text, pill buttons stack vertically |
| Tablet | 768px–1024px | 2-column flexible grid, 24px container padding, 32px section gaps, 16px body text, cards at 100% width |
| Desktop | 1025px–1440px | 12-column grid, 32px container padding, 60px section gaps, 18px headings, max width 1440px |
| Large Desktop | 1441px+ | Center content at 1440px max, maintain consistent spacing, full-width hero sections |

### Touch Targets
- **Minimum Button Height:** 48px
- **Minimum Touch Area:** 44px × 44px
- **Minimum Link Height:** 40px
- **Horizontal Spacing Between Buttons:** 12px
- **Icon Size (Clickable):** 32px minimum, padded to 44px with touch area

### Collapsing Strategy
- **Mobile (320px–767px):** 
  - Collapse 12-column grid to single column
  - Stack navigation horizontally with overflow scroll or hamburger menu
  - Reduce padding from 32px to 16px
  - Scale typography down: H1 28px, H2 16px, body 14px
  - Change button layout from horizontal to vertical stack
  - Full-width cards with 12px padding

- **Tablet (768px–1024px):**
  - Shift to 2-column or 6-column grid
  - Increase padding to 24px
  - Maintain heading sizes but reduce margins
  - Allow cards to display 2 per row where appropriate
  - Navigation remains visible but may compact dropdowns

- **Desktop (1025px+):**
  - Full 12-column grid with 32px gutters
  - Maintain all typography sizes as specified
  - Cards in grid layouts with proper spacing
  - Full navigation bar visible
  - Max width container at 1440px

## 9. Agent Prompt Guide

### Quick Color Reference
- **Primary CTA:** Cyan (`#38ECEC`) — all primary interactive buttons and links
- **Secondary CTA:** Purple (`#6441CC`) — secondary buttons and actions
- **Background:** Deep Navy (`#322066`) — primary container and card backgrounds
- **Dark Background:** Dark Indigo (`#171236`) — alternative dark surface
- **Text (Primary):** Pure White (`#FFFFFF`) — all body text on dark backgrounds
- **Text (Secondary):** Light Gray (`#CCCCCC`) — secondary information and hints
- **Border:** Purple (`#322066`) — card borders and container outlines
- **Success:** Cyan (`#38ECEC`) — success states and positive feedback
- **Error:** Error Red (`#FF2F2F`) — error validation and alerts
- **Warning:** Warning Yellow (`#FFFF2C`) — warning states
- **Heading Text:** Pure White (`#FFFFFF`) — all headings on dark backgrounds
- **Hover State Purple:** Vibrant Purple (`#7D51FF`) — secondary button hover states

### Iteration Guide

1. **Start with dark navy (`#322066`) as the foundational background** for all container-level elements, cards, and surfaces. Layer with dark indigo (`#171236`) for visual depth.

2. **Use cyan (`#38ECEC`) exclusively for primary interactive elements**—buttons, active links, success states, and highlights. This is the energy color that signals action.

3. **Apply purple hierarchy (`#6441CC` → `#7D51FF` → `#9774FF`) for secondary interactions**, dropdowns, alternative buttons, and focus states in descending prominence.

4. **Set all body text to pure white (`#FFFFFF`) on dark backgrounds** and enforce 4.5:1 contrast ratio minimum. Use light gray (`#CCCCCC`) only for secondary content or disabled states.

5. **Implement consistent spacing from the 4px scale**—all gaps, padding, and margins must reference values in the defined system (16px standard for cards, 24px for sections, 60px for major breaks).

6. **Apply border radius from the locked scale only**: 0px (flat), 12px (inputs), 16px (cards), 20px (large containers), 80px (pills), 50% (circles). No arbitrary radius values.

7. **Use SF Pro Display exclusively** with weight 400 for body, 600 for subheadings, and 700 for emphasis. Never deviate from the typography hierarchy table without design review.

8. **Implement shadows sparingly and only for elevation**—use the subtle shadow for navigation (level 1) and elevated shadow for modals/important layers (level 2). No shadows on flat elements.

9. **Test all interactive elements for touch accessibility**—minimum 44px touch target, minimum 48px button height, 12px spacing between touch elements on mobile.

10. **Use transitions consistently (0.2s-0.3s ease) on hover, focus, and active states** for refined feedback without jarring animation. All state changes must feel responsive and smooth.