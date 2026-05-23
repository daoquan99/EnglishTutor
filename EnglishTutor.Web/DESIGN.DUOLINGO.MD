# Design System Inspired by Duolingo

## 1. Visual Theme & Atmosphere

Duolingo's design system embodies a playful, approachable, and energetic learning environment that makes education feel accessible and rewarding. The visual identity combines vibrant, friendly characters with a clean, modern interface that reduces cognitive load while maintaining strong visual hierarchy. Every interaction is designed to feel encouraging and celebratory, reflected through bright accent colors, rounded geometry, and a warm typography system that invites users to engage. The overall mood is optimistic and gamified—emphasizing progress, achievement, and the joy of learning—while maintaining clarity and usability across all devices.

**Key Characteristics**
- Friendly, rounded typography and component design
- High-contrast, vibrant accent colors against neutral backgrounds
- Gamified interaction patterns with celebratory visual feedback
- Clean, spacious layouts prioritizing focus and retention
- Playful character illustrations and iconography
- Science-backed messaging paired with approachable visual language

## 2. Color Palette & Roles

### Primary
- **Duolingo Green** (`#1CB0F6`): Primary call-to-action buttons, key interactive elements, and primary UI accents. Use for primary CTAs, success states, and main navigation highlights.
- **Brand Green** (`#00B086`): Alternative accent and supporting UI elements for visual variety and depth.

### Interactive
- **Link Blue** (`#0000EE`): Hyperlinks and tertiary interactive elements. Used for secondary navigation and in-text actions.
- **Button Light Blue** (`#1CB0F6`): Interactive button states and form focus indicators.

### Neutral Scale
- **Dark Charcoal** (`#3C3C3C`): Primary text color for headings, body copy, and high-contrast UI elements.
- **Off-White** (`#FFFFFF`): Primary background for cards, containers, and surfaces.
- **Light Border** (`#E5E5E5`): Subtle dividers, borders, and low-emphasis separators.

### Surface & Borders
- **Form Border** (`#C1C1C1`): Input field borders and inactive state indicators.
- **Clean White** (`#FFFFFF`): Card backgrounds, content surfaces, and light theme foundations.

## 3. Typography Rules

### Font Family
**Primary:** Din Round (`din-round`, sans-serif, fallback: `'Segoe UI', Roboto, sans-serif`)  
**Secondary:** Feather (`feather`, sans-serif, fallback: `'Segoe UI', Roboto, sans-serif`)

### Hierarchy

| Role | Font | Size | Weight | Line Height | Letter Spacing | Notes |
|------|------|------|--------|-------------|-----------------|-------|
| Display/Hero | Feather | 48px | 700 | normal | normal | Primary page headings, hero sections |
| Heading 1 | Din Round | 32px | 700 | normal | normal | Major section headings, call-to-action text |
| Body | Din Round | 17px | 500 | 24px | normal | Primary body copy, paragraphs, descriptions |
| Button/Label | Din Round | 15px | 700 | normal | normal | Button text, badges, emphasis labels |
| Caption | Din Round | 15px | 700 | normal | normal | Small emphasis text, badges, highlights |

### Principles
- Use **Display** role (48px) for hero sections and major page titles to establish presence and hierarchy
- Default to **Body** (17px, 500 weight) for all descriptive text and paragraph content
- Apply **Button/Label** weight (700) for all interactive text to signal actionability
- Maintain consistent 24px line height for body text to ensure legibility and breathing room
- Use Din Round as the default for all UI text—consistent, friendly, and highly legible
- Reserve Feather for high-impact display headings that need maximum visual presence

## 4. Component Stylings

### Buttons

**Primary Button (Call-to-Action)**
- Background: `#1CB0F6`
- Text Color: `#FFFFFF`
- Font: Din Round, 15px, weight 700
- Padding: `0px 16px`
- Height: `50px`
- Width: `330px` (full-width context) or auto
- Border Radius: `12px`
- Border: `0px solid transparent`
- Box Shadow: `none`
- Line Height: normal
- Hover State: Darken background to `#0099DD`, scale 1.02
- Active State: Darken further to `#007BB3`
- Disabled State: Background `#CCCCCC`, cursor not-allowed

**Secondary Button (Text Link)**
- Background: transparent (`rgba(0, 0, 0, 0)`)
- Text Color: `#1CB0F6`
- Font: Din Round, 15px, weight 700
- Padding: `0px 16px`
- Height: `50px`
- Width: `330px` (full-width context) or auto
- Border Radius: `12px`
- Border: `2px solid transparent`
- Box Shadow: `none`
- Line Height: normal
- Hover State: Background `rgba(28, 176, 246, 0.08)`, text remains `#1CB0F6`
- Active State: Background `rgba(28, 176, 246, 0.15)`

**Tertiary Button (Minimal)**
- Background: transparent (`rgba(0, 0, 0, 0)`)
- Text Color: `#3C3C3C`
- Font: Din Round, 15px, weight 700
- Padding: `0px 16px`
- Height: `50px`
- Width: auto
- Border Radius: `12px`
- Border: `0px solid transparent`
- Box Shadow: `none`
- Line Height: normal
- Hover State: Background `rgba(60, 60, 60, 0.08)`
- Active State: Background `rgba(60, 60, 60, 0.15)`

### Cards & Containers

**Standard Card**
- Background: `#FFFFFF`
- Border: `1px solid #E5E5E5`
- Border Radius: `12px`
- Padding: `24px`
- Box Shadow: `0px 2px 8px rgba(0, 0, 0, 0.04)`
- Hover State: Box shadow becomes `0px 4px 12px rgba(0, 0, 0, 0.08)`, slight scale 1.01

**Feature Card (Highlighted)**
- Background: `#FFFFFF`
- Border: `2px solid #1CB0F6`
- Border Radius: `12px`
- Padding: `24px`
- Box Shadow: `0px 2px 12px rgba(28, 176, 246, 0.12)`

### Inputs & Forms

**Text Input (Default)**
- Background: `#FFFFFF`
- Text Color: `#000000`
- Font: Din Round, 17px, weight 400
- Padding: `12px 16px`
- Height: `40px`
- Border: `1px solid #C1C1C1`
- Border Radius: `0px`
- Box Shadow: `none`
- Line Height: 19.55px
- Focus State: Border color becomes `#1CB0F6`, box shadow `0px 0px 0px 3px rgba(28, 176, 246, 0.1)`
- Placeholder Color: `#999999`
- Disabled State: Background `#F5F5F5`, border `#E5E5E5`, text color `#CCCCCC`

**Input Label**
- Font: Din Round, 15px, weight 700
- Color: `#3C3C3C`
- Margin Bottom: `8px`
- Display: block

### Navigation

**Header Navigation**
- Background: `#FFFFFF`
- Text Color: `#3C3C3C`
- Font: Din Round, 17px, weight 500
- Padding: `16px 24px`
- Height: `70px`
- Border: `0px none`
- Box Shadow: `0px 2px 4px rgba(0, 0, 0, 0.04)`
- Line Height: 20px
- Active Link Color: `#1CB0F6`
- Hover State: Text color darkens to `#1CB0F6`, light background `rgba(28, 176, 246, 0.08)`

**Navigation Link**
- Text Color: `#3C3C3C`
- Font: Din Round, 17px, weight 500
- Hover State: Color becomes `#1CB0F6`
- Active State: Color `#1CB0F6`, bottom border `2px solid #1CB0F6`
- Padding: `8px 12px`
- Line Height: 20px

### Links

**Standard Hyperlink**
- Text Color: `#0000EE`
- Font: Din Round, 17px, weight 500
- Text Decoration: underline (on hover)
- Hover State: Color becomes `#1CB0F6`, text-decoration underline
- Active State: Color `#0000CC`

**CTA Link (Colored)**
- Text Color: `#FFFFFF`
- Font: Din Round, 15px, weight 700
- Background: `#1CB0F6`
- Padding: `0px 20px`
- Height: `44px`
- Border Radius: `12px`
- Line Height: normal
- Hover State: Background `#0099DD`

### Badges

**Standard Badge**
- Background: `#E5E5E5`
- Text Color: `#3C3C3C`
- Font: Din Round, 15px, weight 700
- Padding: `4px 8px`
- Border Radius: `2px`
- Display: inline-block
- Line Height: normal

**Success Badge**
- Background: `#00B086`
- Text Color: `#FFFFFF`
- Font: Din Round, 15px, weight 700
- Padding: `4px 8px`
- Border Radius: `2px`

## 5. Layout Principles

### Spacing System

Base unit: `8px`

**Scale:**
- `8px` — micro spacing (gaps within components)
- `12px` — extra-small spacing (tight gaps, badge spacing)
- `16px` — small spacing (form field margins, button padding)
- `24px` — base spacing (card padding, section gaps)
- `32px` — medium spacing (component separation)
- `40px` — large spacing (section margins)
- `48px` — extra-large spacing (container padding)
- `64px` — hero spacing (major section padding)
- `72px` — massive spacing (hero section top/bottom)
- `80px` — sectional spacing (between major sections)
- `96px` — banner spacing (large container padding)
- `100px` — maximum spacing (extreme separation)

**Usage Context:**
- **8px**: Gap between button icon and text, tight list spacing
- **12px**: Form field gaps, badge margins
- **16px**: Input padding, standard button horizontal padding
- **24px**: Card padding, section spacing, standard gap
- **32px**: Component spacing within layouts
- **40px**: Section margin for clear visual separation
- **48px**: Container padding for main content areas
- **64px**: Hero section top padding
- **72px**: Large hero section padding
- **80px**: Between major page sections
- **96px**: Container horizontal padding for wide layouts
- **100px**: Maximum spacing for distinct layout zones

### Grid & Container

- **Max Width**: `1200px` (standard content container)
- **Column Strategy**: 12-column responsive grid
- **Gutter Width**: `24px` (consistent spacing between columns)
- **Padding**: `24px` (mobile), `48px` (tablet), `96px` (desktop)
- **Section Patterns**: Hero (full-width), content (max-width container), footer (full-width)

### Whitespace Philosophy

Duolingo's spacing approach prioritizes breathing room and visual clarity. Generous whitespace prevents cognitive overload and creates a calm, inviting interface. Hero sections use maximum padding (`96px` to `100px`) to establish presence, while component spacing remains consistent at `24px`. The design avoids visual clutter through strategic use of empty space, allowing typography and color to guide attention naturally.

### Border Radius Scale

- `0px` — Form inputs, table cells, strict geometric elements
- `2px` — Badges, small labels, minimal rounding
- `12px` — Buttons, cards, modals, primary UI elements
- `50%` — Circular elements, avatar placeholders, full-round badges

## 6. Depth & Elevation

| Level | Treatment | Use |
|-------|-----------|-----|
| Raised (Hover) | `0px 4px 12px rgba(0, 0, 0, 0.08)` | Card hover states, lifted buttons, interactive elevation |
| Elevated (Base) | `0px 2px 8px rgba(0, 0, 0, 0.04)` | Standard cards, default container shadows |
| Minimal | `0px 2px 4px rgba(0, 0, 0, 0.04)` | Navigation bars, subtle separation |
| Feature Highlight | `0px 2px 12px rgba(28, 176, 246, 0.12)` | Feature cards, primary highlighted containers |
| None | `none` | Text inputs (focus state uses color instead), buttons (color provides depth) |

**Shadow Philosophy:**

Duolingo employs subtle, delicate shadows that create depth without visual weight. Shadows are used sparingly to lift elements slightly off the background, creating a layered appearance that guides focus. The primary shadow (`0px 2px 8px rgba(0, 0, 0, 0.04)`) is nearly imperceptible, maintaining the clean aesthetic while providing spatial context. Hover states amplify shadows slightly to signal interactivity, and feature cards use tinted shadows that reference the primary brand color for visual cohesion.

## 7. Do's and Don'ts

### Do
- **Use Din Round for all UI text.** It's friendly, consistent, and highly legible across all sizes.
- **Apply `#1CB0F6` to all primary CTAs.** Consistent color signaling ensures users immediately recognize important actions.
- **Maintain `24px` spacing between major sections.** This establishes rhythm and prevents layout chaos.
- **Use bold, 700-weight text for all buttons and labels.** Weight signals actionability and hierarchy.
- **Keep card padding at `24px` minimum.** Generous internal spacing improves content readability.
- **Use full-width buttons on mobile.** Larger touch targets improve usability on small screens.
- **Apply subtle shadows (`0px 2px 8px rgba(0, 0, 0, 0.04)`) to cards by default.** This creates depth without visual clutter.
- **Center align hero section text and CTAs.** This creates a welcoming, focused entry point.
- **Use `#E5E5E5` borders for form inputs.** Light borders maintain visual hierarchy without harsh contrast.

### Don't
- **Don't use more than three font sizes in a single view.** Limit typography scale to 48px, 32px, and 17px for clarity.
- **Don't apply shadows to buttons.** Color alone should provide depth; shadows add unnecessary weight.
- **Don't use rounded corners smaller than `2px`.** Duolingo's minimum radius (2px on badges) maintains brand consistency.
- **Don't mix Link Blue (`#0000EE`) with primary CTAs.** Reserve it for tertiary navigation only; it competes with primary brand green.
- **Don't add more than `2px` borders to cards.** Thick borders create visual heaviness; use subtle separators instead.
- **Don't use full-width CTAs on desktop.** Limit CTA width to `330px` for optimal focus and click targeting.
- **Don't place text directly on brand green without sufficient contrast.** Always ensure text color provides WCAG AA contrast (minimum 4.5:1).
- **Don't overuse the accent green (`#00B086`). Reserve it for supporting elements; use primary blue (`#1CB0F6`) for main CTAs.**
- **Don't reduce input height below `40px`.** Small inputs feel cramped and reduce tap target size on mobile.

## 8. Responsive Behavior

### Breakpoints

| Name | Width | Key Changes |
|------|-------|-------------|
| Mobile | 320px–767px | Single-column layout, full-width buttons (`330px` on 360px+ screens), padding `16px` |
| Tablet | 768px–1023px | Two-column layout, buttons `200px`–`250px`, padding `32px`, navigation shifts to hamburger menu |
| Desktop | 1024px+ | Multi-column layout, max-width container `1200px`, full navigation visible, padding `48px`–`96px` |

### Touch Targets

- **Minimum Button Height**: `50px` (standard interactive element)
- **Minimum Button Width**: `48px` (small iconographic buttons)
- **Link Tap Area**: `44px` height minimum
- **Input Field Height**: `40px` minimum
- **Spacing Between Buttons**: `12px` minimum to prevent accidental taps

### Collapsing Strategy

- **Mobile (320px–767px)**: Stack all content vertically. Full-width buttons, `16px` padding, single-column cards. Navigation collapses to hamburger. Hero sections remain full-width with `48px` padding.
- **Tablet (768px–1023px)**: Two-column grid layout for card grids. Buttons expand to `200px`–`250px` width. Padding increases to `32px`. Hero text remains centered but may increase to 40px width max.
- **Desktop (1024px+)**: Multi-column layouts, max-width `1200px` container centered. Full header navigation visible. Padding `96px` for hero sections, `48px` for standard content. Buttons remain `330px` for primary CTAs, smaller for secondary actions.

## 9. Agent Prompt Guide

### Quick Color Reference
- **Primary CTA**: Duolingo Green (`#1CB0F6`)
- **Secondary CTA**: Link Blue (`#0000EE`)
- **Accent**: Brand Green (`#00B086`)
- **Background**: Off-White (`#FFFFFF`)
- **Text (Primary)**: Dark Charcoal (`#3C3C3C`)
- **Text (Secondary)**: Link Blue (`#0000EE`)
- **Borders**: Light Border (`#E5E5E5`)
- **Form Borders**: Form Border (`#C1C1C1`)
- **Heading Text**: Dark Charcoal (`#3C3C3C`)

### Iteration Guide

1. **All buttons default to Din Round, 15px, weight 700.** No exceptions. This ensures consistent, recognizable interactive elements.
2. **Primary CTAs are always `#1CB0F6` background with `#FFFFFF` text.** Full-width on mobile (`330px`), auto-width on desktop, `50px` height minimum.
3. **Use `24px` padding inside cards and containers.** This is the base spacing unit for all internal content.
4. **All text defaults to `17px` body size, `#3C3C3C` color, 24px line height.** Increase to `32px` for headings, reduce to `15px` only for buttons, labels, and captions.
5. **Border radius is `12px` for all buttons and cards.** Use `0px` for form inputs, `2px` for badges only.
6. **Shadows on cards are `0px 2px 8px rgba(0, 0, 0, 0.04)`.** Never apply to buttons; hover states use color darkening instead.
7. **Form inputs have `1px solid #C1C1C1` borders and `40px` height minimum.** Focus state adds `#1CB0F6` border and `rgba(28, 176, 246, 0.1)` shadow.
8. **Navigation text is `17px`, weight 500, `#3C3C3C`.** Active state becomes `#1CB0F6` with underline.
9. **Hero sections use 48px display heading (Feather font) centered, with `96px` top/bottom padding.** Subtext is 17px body, centered, with maximum `600px` width.
10. **Always maintain WCAG AA contrast (4.5:1 minimum)** between text and background. Test `#1CB0F6` text on light backgrounds carefully; use `#0000EE` or `#3C3C3C` if contrast fails.