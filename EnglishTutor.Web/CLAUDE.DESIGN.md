# Design System Inspired by Meow English

## 1. Visual Theme & Atmosphere

The Meow English design system embodies an elegant, literary aesthetic inspired by classical libraries and British intellectual heritage. The visual language combines warm, inviting library architecture with modern pedagogical clarity, creating a sophisticated yet approachable learning environment. The design philosophy emphasizes focus and clarity through generous whitespace, carefully controlled contrast, and a warm color palette anchored by deep charcoal and cream tones. The mascot "Meow" — a stylized cat character — brings personality and warmth, making language learning feel friendly and achievable. This system prioritizes readability, progressive disclosure of features, and visual hierarchy that guides users naturally through learning journeys without overwhelming cognitive load.

**Key Characteristics**

- Warm, literary aesthetic with classical architectural undertones
- High contrast between dark text and light backgrounds for reading clarity
- Generous whitespace and breathing room between elements
- Rounded, pill-shaped interactive buttons that feel inviting and tactile
- Orange/amber accent color for warmth and call-to-action emphasis
- Serif headings for authority and elegance; sans-serif body for modern clarity
- Illustrated backgrounds that create immersive, thematic contexts
- Playful mascot integration balancing professionalism with approachability
- Consistent depth and shadow effects for subtle layering
- Responsive to varied learning methods (flashcards, audio, interactive content)

## 2. Color Palette & Roles

### Primary
- **Deep Charcoal** (`#151210`): Primary text color for all body copy, headings, and foundational UI elements. Establishes strong visual hierarchy and ensures accessibility-grade contrast on light backgrounds.
- **Cream White** (`#FFFFFF`): Primary background and surface color. Used for cards, containers, and clean negative space that promotes focus and readability.

### Accent Colors
- **Warm Orange** (`#D97706`): Primary call-to-action button backgrounds, emphasis points, and interactive highlights. Conveys energy, warmth, and invitation to learning. Used with `rgba(217, 119, 6, 0.4)` glow effect for subtle depth.
- **Sunset Amber** (`#CA8A04`): Secondary interactive states and hover feedback, creating visual continuity with primary orange while offering distinction.

### Interactive
- **Button Primary** (`#D97706`): Solid background for primary actions (e.g., "Start Learning Now"). Full opacity with glowing shadow effect.
- **Button Secondary** (`oklch(0.555 0.163 48.998)` / approx. `#C97B3A`): Alternative action buttons with refined shadow treatment for visual hierarchy depth.
- **Link Primary** (`oklch(0.75 0.15 65)` / approx. `#F5D373`): Navigation and tertiary links with soft glow shadow for click affordance.
- **Link Secondary** (`rgba(255, 255, 255, 0.7)`): Low-emphasis navigation and metadata links, reducing cognitive load on secondary pathways.

### Neutral Scale
- **Neutral Dark** (`#151210`): Text on light surfaces; primary UI foreground.
- **Neutral Medium** (`rgba(255, 255, 255, 0.7)`): Secondary text, metadata, and hint copy; reduces emphasis while maintaining legibility.
- **Neutral Light** (`#FFFFFF`): Primary background and card surfaces.
- **Neutral Overlay Dark** (`rgba(0, 0, 0, 0.1)`): Subtle backgrounds for input fields and secondary containers; creates visual separation without heaviness.

### Surface & Borders
- **Card Surface** (`#FFFFFF`): Standard container background with drop shadow for depth elevation.
- **Border Subtle** (`oklch(0.4 0.1 60 / 0.3)`): Delicate borders for form inputs and subtle dividers; maintains visual continuity without visual noise.
- **Background Dark** (`#1A1612`): Optional deep background for atmospheric sections; inferred from library imagery.

### Semantic / Status
- **Success** (inferred `#10B981`): Positive feedback, completed lessons, streak achievements (not explicitly extracted; follow brand warmth and use muted teal).
- **Warning** (inferred `#F59E0B`): Alerts, streak interruptions, or missed study milestones (align with accent orange family).
- **Shadow Glow Warm** (`rgba(217, 119, 6, 0.4) 0px 0px 30px 0px`): Ambient glow on primary buttons for warmth and depth cue.

## 3. Typography Rules

### Font Family
- **Primary Serif**: `ui-serif`, system font fallbacks `("Garamond", "Crimson Text", "Georgia", serif)`. Used for display and headings to convey authority and elegance.
- **Secondary Sans-Serif**: `Manrope`, fallbacks `(-apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif)`. Used for body, buttons, and UI text for modern clarity and legibility.
- **Tertiary Sans-Serif**: `Be Vietnam Pro`, fallbacks `(ui-sans-serif, system-ui, sans-serif)`. Used for links and accent typography; adds Vietnamese language support and distinctive character.

### Hierarchy

| Role | Font | Size | Weight | Line Height | Letter Spacing | Notes |
|------|------|------|--------|-------------|----------------|-------|
| Display / H1 | ui-serif | 80px | 700 | 88px | 0px | Hero headlines, page titles. Maximum visual impact. |
| Heading 2 / H2 | ui-serif | 36px | 700 | 40px | 0px | Section headers, major content divisions. |
| Heading 3 / H3 | ui-serif | 18px | 700 | 28px | 0px | Card titles, feature headers, subsections. |
| Body / P | Manrope | 16px | 400 | 24px | 0px | Primary reading copy, descriptions, long-form content. |
| Button / CTA | Manrope | 16px | 600 | 24px | 0px | Primary call-to-action buttons. |
| Button Secondary | Manrope | 14px | 500 | 20px | 0px | Secondary buttons, tertiary actions. |
| Link / Navigation | Be Vietnam Pro | 14px | 700 | 23.8px | 0px | Primary links, navigation items. Bold weight emphasizes interactivity. |
| Link Secondary | Manrope | 14px | 500 | 20px | 0px | Secondary navigation, breadcrumbs, metadata links. |
| Caption / Metadata | Manrope | 12px | 400 | 18px | 0px | Image captions, timestamps, secondary metadata. |
| Overline / Tag | Be Vietnam Pro | 12px | 600 | 18px | 0.5px | Tags, badges, status labels. |
| Code / Monospace | `Monaco` or `SF Mono` | 13px | 400 | 20px | 0px | Code blocks, technical content (if applicable). |

### Principles

- **Serif for Hierarchy**: Serif headings establish authority and elegance, guiding visual flow and content importance.
- **Sans-Serif for Clarity**: Body and UI text in Manrope ensures modern readability and accessibility across screen sizes.
- **Contrast Through Weight**: Typography hierarchy relies on weight and size rather than color shifts, minimizing cognitive load.
- **Generous Leading**: Line heights exceed standard (88px for 80px headlines) to create breathing room and reduce visual tension.
- **Vietnamese Locale Support**: Be Vietnam Pro link font supports Vietnamese diacritics and cultural localization.
- **Unified Line Height Scale**: Heights follow `+8px` increments (24px, 28px, 40px, 88px) creating harmonic rhythm.

## 4. Component Stylings

### Buttons

#### Primary Button
- **Background**: `#D97706`
- **Text Color**: `#FFFFFF`
- **Font**: Manrope, 16px, weight 600
- **Padding**: `14px 32px` (vertical × horizontal)
- **Border Radius**: `9999px` (fully rounded/pill shape)
- **Border**: `0px solid` (no border)
- **Height**: `54px`
- **Box Shadow**: `rgba(217, 119, 6, 0.4) 0px 0px 30px 0px` (warm glow)
- **Hover State**: Darken background to `#B85C0B`, increase glow opacity to `rgba(217, 119, 6, 0.6)`
- **Active State**: `#A45409`, reduce glow to `rgba(217, 119, 6, 0.2)`
- **Disabled State**: Background `#D1D5DB`, text `#9CA3AF`, no shadow

#### Secondary Button
- **Background**: `#C97B3A` (oklch equivalent)
- **Text Color**: `#FFFFFF`
- **Font**: Manrope, 14px, weight 500
- **Padding**: `16px 32px`
- **Border Radius**: `9999px`
- **Border**: `0px solid`
- **Height**: `52px`
- **Box Shadow**: `rgba(0, 0, 0, 0.1) 0px 20px 25px -5px, rgba(0, 0, 0, 0.1) 0px 8px 10px -6px` (depth shadow)
- **Hover State**: Background `#B56F2F`, shadow opacity +10%
- **Active State**: Background `#A05F24`, shadow opacity -10%
- **Disabled State**: Background `#E5E7EB`, text `#9CA3AF`, no shadow

#### Ghost Button (Tertiary)
- **Background**: `rgba(0, 0, 0, 0)` (transparent)
- **Text Color**: `rgba(255, 255, 255, 0.7)`
- **Font**: Manrope, 14px, weight 500
- **Padding**: `8px 16px`
- **Border Radius**: `9999px`
- **Border**: `0px solid` (no visible border; optional `1px solid rgba(255, 255, 255, 0.3)`)
- **Height**: `36px`
- **Box Shadow**: `none`
- **Hover State**: Background `rgba(255, 255, 255, 0.1)`, text `rgba(255, 255, 255, 0.9)`
- **Active State**: Background `rgba(255, 255, 255, 0.15)`, text `#FFFFFF`

### Cards & Containers

#### Standard Card
- **Background**: `#FFFFFF`
- **Border**: `0px solid`
- **Border Radius**: `12px`
- **Padding**: `24px` or `32px` (interior spacing)
- **Box Shadow**: `rgba(0, 0, 0, 0.1) 0px 10px 15px -3px, rgba(0, 0, 0, 0.1) 0px 4px 6px -4px` (subtle lift)
- **Text Color**: `#151210`
- **Use**: Feature blocks, lesson cards, user statistics displays

#### Elevated Card
- **Background**: `#FFFFFF`
- **Border**: `0px solid`
- **Border Radius**: `12px`
- **Padding**: `24px`
- **Box Shadow**: `rgba(0, 0, 0, 0.15) 0px 20px 25px -5px, rgba(0, 0, 0, 0.1) 0px 10px 10px -5px` (higher lift)
- **Use**: Modal dialogs, featured content, overlay surfaces

#### Subtle Container
- **Background**: `rgba(0, 0, 0, 0.02)` (nearly white)
- **Border**: `1px solid oklch(0.4 0.1 60 / 0.3)`
- **Border Radius**: `8px`
- **Padding**: `16px`
- **Box Shadow**: `none`
- **Use**: Form sections, grouped information, low-hierarchy containers

### Inputs & Forms

#### Text Input
- **Background**: `#FFFFFF`
- **Border**: `1px solid oklch(0.4 0.1 60 / 0.3)` (subtle line)
- **Border Radius**: `8px`
- **Padding**: `12px 16px`
- **Font**: Manrope, 16px, weight 400
- **Text Color**: `#151210`
- **Placeholder Color**: `rgba(21, 18, 16, 0.5)`
- **Height**: `44px`
- **Focus State**: Border `1px solid #D97706`, box-shadow `0 0 0 3px rgba(217, 119, 6, 0.1)`, background `#FFFBF0`
- **Error State**: Border `1px solid #EF4444`, text `#DC2626`
- **Disabled State**: Background `#F3F4F6`, border `1px solid #E5E7EB`, text `#9CA3AF`

#### Select / Dropdown
- **Background**: `#FFFFFF`
- **Border**: `1px solid oklch(0.4 0.1 60 / 0.3)`
- **Border Radius**: `8px`
- **Padding**: `12px 16px`
- **Font**: Manrope, 16px, weight 400
- **Height**: `44px`
- **Focus State**: Same as text input focus
- **Open State**: Border `1px solid #D97706`, shadow `rgba(217, 119, 6, 0.1) 0px 0px 0px 3px`

#### Checkbox
- **Size**: `20px × 20px`
- **Border Radius**: `4px`
- **Border**: `2px solid oklch(0.4 0.1 60 / 0.3)`
- **Background (Unchecked)**: `#FFFFFF`
- **Background (Checked)**: `#D97706`
- **Checkmark Color**: `#FFFFFF`
- **Focus State**: Outline `2px solid #D97706`, offset `2px`

### Navigation

#### Header Navigation
- **Background**: `#FFFFFF` or `rgba(255, 255, 255, 0.98)` (slight transparency for backdrop blur)
- **Border Bottom**: `1px solid rgba(0, 0, 0, 0.05)`
- **Padding**: `16px 24px` (vertical × horizontal, adjustable per layout)
- **Height**: `64px` (excluding logo/brand area)
- **Logo Font**: Be Vietnam Pro, 20px, weight 700, color `#151210`
- **Link Color**: `#151210`
- **Link Font**: Manrope, 14px, weight 500
- **Link Padding**: `8px 16px`
- **Link Hover State**: Background `rgba(217, 119, 6, 0.1)`, border-radius `4px`
- **Active Link**: Color `#D97706`, border-bottom `2px solid #D97706`

#### Breadcrumb Navigation
- **Font**: Be Vietnam Pro, 12px, weight 500
- **Separator**: `/` or `>` in `#9CA3AF`
- **Item Padding**: `4px 8px`
- **Current Item Color**: `#151210`, weight 600
- **Previous Item Color**: `#6B7280`
- **Hover State**: Previous items color `#D97706`

### Badges

#### Status Badge
- **Background**: Semantic color (e.g., `#D1FAE5` for success)
- **Text Color**: Semantic dark (e.g., `#065F46` for success)
- **Font**: Manrope, 12px, weight 600
- **Padding**: `6px 12px`
- **Border Radius**: `9999px`
- **Use**: Streak status, lesson completion, difficulty level

#### Feature Badge
- **Background**: `#FEF3C7` (light amber)
- **Text Color**: `#92400E` (dark amber)
- **Font**: Be Vietnam Pro, 11px, weight 700
- **Padding**: `4px 10px`
- **Border Radius**: `4px`
- **Letter Spacing**: `0.5px`

## 5. Layout Principles

### Spacing System

**Base Unit**: `4px`

**Scale**: All spacing values derive from the 4px base unit:
- `4px` (1×) — Minimal spacing, tight adjacency
- `8px` (2×) — Padding within compact components
- `12px` (3×) — Gap between closely related elements
- `16px` (4×) — Standard padding and gap between grouped elements
- `24px` (6×) — Interior padding in cards and containers
- `32px` (8×) — Section-level gaps, component spacing
- `40px` (10×) — Medium vertical spacing
- `48px` (12×) — Larger section separation
- `64px` (16×) — Hero section padding, major content shifts
- `80px` (20×) — Large vertical breathing room
- `96px` (24×) — Between major layout sections
- `128px` (32×) — Full-width section separation, page breaks

**Usage Context**:
- `4px–8px`: Button internal gaps, icon spacing, tight grids
- `12px–16px`: List item gaps, form field spacing, card grids
- `24px–32px`: Card padding, container interior margins
- `40px–48px`: Section headers to content
- `64px–96px`: Between major page sections (hero to features, features to footer)
- `128px`: Full-page section breaks

### Grid & Container

- **Max Width**: `1280px` (desktop maximum container width)
- **Gutters**: `24px` (sides) on tablet, `32px` on desktop
- **Column Strategy**: 12-column responsive grid for flexible layouts. Collapses to 6-column (tablet) and 4-column (mobile).
- **Section Padding**: Top/bottom `64px` (desktop), `48px` (tablet), `32px` (mobile)
- **Content Padding**: Horizontal `32px` desktop / `24px` tablet / `16px` mobile
- **Hero Container**: Full viewport width with `80px` padding (desktop), `48px` (tablet), `24px` (mobile). Vertical centering with min-height `80vh`.
- **Card Grid**: 3 columns (desktop), 2 (tablet), 1 (mobile) with `24px` gap

### Whitespace Philosophy

Generous, intentional whitespace creates visual breathing room and reduces cognitive load. The design embraces negative space to guide focus toward content hierarchy. Large empty areas frame key elements, particularly hero sections and call-to-action buttons. Whitespace between sections grows progressively from top to bottom, mirroring reading flow and building toward conclusions. Cards and containers maintain consistent internal padding (`24px` minimum) to prevent crowded information density. This spacing strategy aligns with literary and gallery aesthetics, where curated white space enhances elegance and approachability.

### Border Radius Scale

- `4px`: Subtle, minimal rounding. Used for form inputs, badges, small UI components.
- `8px`: Moderate rounding. Default for standard cards, buttons, containers.
- `9999px`: Full pill shape. Applied to all primary buttons, navigation pills, floating action elements.
- `12px`: Generous rounding. Feature cards, larger containers, elevated surfaces.
- `16px`: Extra soft. Large hero sections, full-screen overlays (optional).

## 6. Depth & Elevation

| Level | Treatment | Use |
|-------|-----------|-----|
| 0 (Flat) | No shadow, solid background | Text layers, content, minimal depth |
| 1 (Subtle) | `rgba(0, 0, 0, 0.05) 0px 1px 2px 0px` | Form inputs, secondary cards, borders only |
| 2 (Raised) | `rgba(0, 0, 0, 0.1) 0px 4px 6px -4px` | Standard cards, input focus states |
| 3 (Floating) | `rgba(0, 0, 0, 0.1) 0px 10px 15px -3px, rgba(0, 0, 0, 0.1) 0px 4px 6px -4px` | Card hover, elevated surfaces, primary containers |
| 4 (Lifted) | `rgba(0, 0, 0, 0.15) 0px 20px 25px -5px, rgba(0, 0, 0, 0.1) 0px 10px 10px -5px` | Modals, overlays, feature highlights |
| Glow (Warm) | `rgba(217, 119, 6, 0.4) 0px 0px 30px 0px` | Primary buttons, accent emphasis, warmth cue |

**Shadow Philosophy**: Shadows are understated, using soft blur radii and low opacity values to suggest depth without visual heaviness. The warm glow effect (`rgba(217, 119, 6, 0.4)`) creates ambient warmth on primary interactive elements, evoking the cozy library aesthetic. Multi-layered shadows (combining two 2–3 shadow values) create sophisticated depth for elevated states without appearing oversaturated. Shadows remain consistent in color temperature (neutral black with warm accents), reinforcing the brand's warm, inviting aesthetic. Shadows are disabled on ghost buttons and transparent overlays to reduce visual noise.

## 7. Do's and Don'ts

### Do

- **Use the 4px spacing scale consistently**. Every margin, padding, and gap value must align with the scale (4px, 8px, 12px, 16px, 24px, 32px, 40px, 48px, 64px, 80px, 96px, 128px).
- **Maintain text contrast ratios** of at least 4.5:1 (AAA standard). The dark charcoal (`#151210`) on white (`#FFFFFF`) provides 15:1 ratio; orange (`#D97706`) on white provides 6.2:1.
- **Apply pill-shaped buttons** (`border-radius: 9999px`) to all primary and secondary call-to-action buttons for consistency and inviting tactility.
- **Use warm orange** (`#D97706`) exclusively for primary actions and primary accent highlights. Secondary emphasis uses softer amber or neutral tones.
- **Wrap headings in serif fonts** (ui-serif) for authority and elegance. Reserve sans-serif for body copy and UI text.
- **Layer shadows for depth progression**. Use subtle shadows (Level 1–2) for standard surfaces, floating shadows (Level 3) for hover states, and lifted shadows (Level 4) for modals and overlays.
- **Provide visual feedback on interactive elements**. Buttons must show hover (background shift + shadow intensify), active (darker background), and focus (glow outline) states.
- **Pair generous padding** (`24px`–`32px`) with single-column or two-column layouts to prevent information density and ease scanning.
- **Use the hierarchy table strictly**. Assign heading roles (H1–H3) based on visual importance, not semantic nesting in HTML.
- **Employ open, accessible color combinations**. Avoid relying on color alone to convey meaning; use text, icons, and shape variation as fallbacks.

### Don't

- **Don't use rounded buttons** with radius values below `9999px`. Pill shapes are intentional branding; irregular rounding dilutes consistency.
- **Don't exceed `1280px` max-width** for desktop container widths. Wider layouts reduce readability and overwhelm focus on content.
- **Don't place primary call-to-action buttons** in colors other than orange (`#D97706`). Secondary actions use softer tones; neutrals are reserved for navigation and metadata.
- **Don't apply shadows to ghost buttons** or transparent overlays. Shadows are reserved for elevated, opaque surfaces.
- **Don't use less than `24px` padding** inside cards or containers. Tighter spacing creates visual crowding and reduces approachability.
- **Don't mix serif and sans-serif fonts** within a single heading or button. Maintain clear font role separation: serif for hierarchy, sans-serif for clarity.
- **Don't reduce line-height below the specified values** in the typography hierarchy. The generous leading (88px for 80px headlines) is deliberate and aids both elegance and accessibility.
- **Don't use text colors other than `#151210` (dark charcoal) or `#FFFFFF` (white) for primary content** without semantic justification (e.g., status colors, interactive states).
- **Don't apply focus outlines thinner than `2px`** or in colors that don't meet WCAG contrast ratios. All focus states must be keyboard-accessible and visible.
- **Don't override spacing and radius values** for one-off designs. Deviations fragment the design system and reduce consistency across pages.

## 8. Responsive Behavior

### Breakpoints

| Name | Width | Key Changes |
|------|-------|------------|
| Mobile | 320px–639px | Single-column layout, 16px side padding, 32px section padding, full-width cards, larger touch targets (48px minimum), H1 48px, smaller gutter gaps (12px) |
| Tablet | 640px–1023px | Two-column grids, 24px side padding, 48px section padding, 24px card gaps, H1 64px, moderate touch targets (44px) |
| Desktop | 1024px+ | Three-column grids, 32px side padding, 64px section padding, 32px card gaps, H1 80px, standard touch targets (40px+) |
| Wide | 1280px+ | Max-width container `1280px` centered, consistent spacing as desktop, optional four-column grids for feature matrices |

### Touch Targets

- **Minimum Touch Target**: `44px × 44px` (recommended by WCAG and iOS HIG)
- **Buttons**: `52px`–`54px` height to comfortably exceed minimum
- **Links in Navigation**: `40px` × 40px (height × inline width via padding)
- **Form Inputs**: `44px` height, minimum
- **Icon Buttons**: `40px` × `40px` square, centered icon at `20px` × `20px`
- **Checkboxes & Radios**: `20px` × `20px` internal, `44px` × `44px` click zone (via padding)
- **Mobile Navigation Items**: `48px` height for thumb-friendly spacing

### Collapsing Strategy

- **Hero Section**: Full viewport width (100vw) on mobile, centered max-width on desktop. Padding increases from `24px` (mobile) → `48px` (tablet) → `80px` (desktop).
- **Navigation Bar**: Full-width sticky header (stays fixed at top). Logo and nav links stack vertically into off-canvas drawer on mobile (<640px), horizontal navigation on tablet+.
- **Card Grids**: Collapse from 3 columns (desktop) → 2 columns (tablet) → 1 column (mobile). Gap reduces `32px` → `24px` → `16px`.
- **Feature Sections**: Two-column layout (image + text) on desktop; stacks vertically (image above text or text above image alternating) on tablet; single column on mobile.
- **Typography Scaling**: 
  - H1: 80px (desktop) → 64px (tablet) → 48px (mobile)
  - H2: 36px (desktop) → 28px (tablet) → 24px (mobile)
  - Body: 16px (desktop) → 16px (tablet) → 15px (mobile, reduced for spacing efficiency)
- **Button Sizing**: Full-width (`100%`) on mobile/tablet (<768px), auto width on desktop.
- **Padding Collapse**: Interior padding in cards/containers reduces `32px` (desktop) → `24px` (tablet) → `16px` (mobile).
- **Section Margins**: Top/bottom section spacing reduces `96px` (desktop) → `64px` (tablet) → `48px` (mobile).

## 9. Agent Prompt Guide

### Quick Color Reference

- **Primary CTA Background**: Warm Orange (`#D97706`)
- **Primary CTA Text**: Cream White (`#FFFFFF`)
- **Primary CTA Glow**: `rgba(217, 119, 6, 0.4) 0px 0px 30px 0px`
- **Primary Text / Heading**: Deep Charcoal (`#151210`)
- **Background / Card Surface**: Cream White (`#FFFFFF`)
- **Navigation / Secondary Text**: Deep Charcoal (`#151210`), reduced opacity `0.7` for secondary
- **Input Border**: `oklch(0.4 0.1 60 / 0.3)` (subtle gray)
- **Input Focus State**: Orange (`#D97706`) border + `rgba(217, 119, 6, 0.1)` shadow
- **Accent / Highlight**: Warm Orange (`#D97706`) or Sunset Amber (`#CA8A04`)
- **Success Semantic**: `#10B981` (muted teal, inferred)
- **Warning Semantic**: `#F59E0B` (amber, aligned with primary orange)
- **Error Semantic**: `#EF4444` (red)
- **Neutral Overlay**: `rgba(0, 0, 0, 0.02)` to `rgba(0, 0, 0, 0.1)` for subtle layers

### Iteration Guide

1. **Establish Color Roles First**: Assign hex values from the palette (Warm Orange for CTAs, Deep Charcoal for text, Cream White for surfaces). Use orange glows (`rgba(217, 119, 6, 0.4)`) on primary buttons exclusively.

2. **Apply Typography Hierarchy Strictly**: Use the hierarchy table to assign roles (H1–H3 for headings, Body for copy, Button for CTAs). Swap fonts by role (serif for headings, sans-serif for UI text). Never reduce line-height below specified values.

3. **Implement Spacing from the 4px Scale**: All margins, padding, and gaps must align with `{4, 8, 12, 16, 24, 32, 40, 48, 64, 80, 96, 128}px`. Card padding defaults to `24px`–`32px`; section gaps start at `64px`.

4. **Round Interactive Elements Aggressively**: Buttons use `border-radius: 9999px` (pill shape). Cards use `12px`. Inputs use `8px`. Pill shapes are non-negotiable for button branding.

5. **Layer Shadows for Depth Progression**: Standard cards use Level 3 shadow (`rgba(0, 0, 0, 0.1) 0px 10px 15px -3px, rgba(0, 0, 0, 0.1) 0px 4px 6px -4px`). Hovered/elevated surfaces use Level 4. Ghost buttons use no shadow. Primary buttons add warm glow.

6. **Ensure Keyboard Accessibility**: All interactive elements require visible focus states. Use `outline: 2px solid #D97706` or `box-shadow: 0 0 0 3px rgba(217, 119, 6, 0.1)` for focus rings. Maintain 4.5:1 text contrast minimum.

7. **Test Responsive Collapsing**: At mobile (`<640px`), layouts collapse to single-column with full-width buttons. Padding reduces from `32px` → `16px`. Typography reduces H1 from 80px → 48px. Verify touch targets stay ≥44px.

8. **Limit Color Palette Strictly**: Use only the defined palette. If a new semantic color is needed (e.g., info blue), infer from brand warmth and status convention; document additions in design file updates.

9. **Validate Component Specs Against Template**: Every button, input, card, and link must match the exact CSS properties in Section 4 (Component Stylings). Hover/active/focus states must be implemented; disabled states must follow spec.

10. **Maintain Whitespace Philosophy**: Cards and sections should feel open and breathing. Never use padding below `16px` in containers; prefer `24px` minimum. Section gaps start at `64px` and increase to `96px`–`128px` for major divisions. Whitespace is not wasted space; it is intentional design.