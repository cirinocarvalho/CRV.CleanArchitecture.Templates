// ─────────────────────────────────────────────
// Help center content
// ─────────────────────────────────────────────
// One entry per help route. Help.vue looks the current route path up here and
// renders it through SimplePage, so every topic shares one layout.
//
// This is starter copy describing what the template generated. Rewrite each
// page for your application — the markers below show where. Adding a topic
// means three edits: an entry here, a link in help-sidebar.ts, and a path in
// the help route list in src/router/index.ts.
//
// `content` is authored markup rendered with v-html. It is trusted precisely
// because it lives in this file. Never feed it API data or user input.

export interface HelpPage {
  title: string;
  description?: string;
  content: string;
}

export const helpContent: Record<string, HelpPage> = {
  "/help": {
    title: "Help",
    description:
      "How to use StarterApp, and where to go when something isn't working.",
    content: `
      <!-- Replace this overview with what your application does. -->
      <h2>Overview</h2>
      <p>This help center is part of the application shell. Every topic in the menu is a static page you own — edit the copy in <code>src/assets/help-content.ts</code> as the application takes shape.</p>
      <h3>In this help center</h3>
      <ul>
        <li><strong>Getting Started</strong> — creating an account and signing in.</li>
        <li><strong>Attachments</strong> — uploading files and managing them.</li>
        <li><strong>FAQ</strong> — short answers to common questions.</li>
        <li><strong>Contact Support</strong> — how to reach a person.</li>
        <li><strong>Policies</strong> — terms of use, accessibility, and privacy.</li>
      </ul>
      <p>Use the menu on the left to open a topic.</p>
    `,
  },

  //#if (useAuth)
  "/help/getting-started": {
    title: "Getting Started",
    description: "Get access to StarterApp and sign in for the first time.",
    content: `
      <!-- Replace this with your own access process. -->
      <p>You need an account before you can use StarterApp. Once you are signed in, the navigation at the top of the page takes you to everything your account is permitted to see — what appears there depends on the roles assigned to you.</p>
      <h3>In this section</h3>
      <ul>
        <li>Creating an Account</li>
        <li>Signing In</li>
      </ul>
    `,
  },

  //#if (useLocalIdentity)
  "/help/getting-started/account": {
    title: "Creating an Account",
    description: "Register, then confirm your email address.",
    content: `
      <!-- Replace this if your registration process differs. -->
      <h3>Register</h3>
      <p>Open the <strong>Register</strong> page from the sign-in screen and provide your name, email address, and a password. Your email address is your user name, so use one you can receive mail at.</p>
      <h3>Password requirements</h3>
      <p>The registration form lists the password rules and shows which ones you have met as you type. Passwords are stored only as a salted hash — nobody, including an administrator, can read yours back.</p>
      <h3>If you forget your password</h3>
      <p>Use <strong>Forgot password</strong> on the sign-in screen. A reset link is emailed to you and expires after a short period; request a fresh one if it lapses. Once signed in, you can change your password at any time from the account menu.</p>
    `,
  },

  //#endif
  "/help/getting-started/signing-in": {
    title: "Signing In",
    //#if (useLocalIdentity)
    description: "Sign in with your email address and password.",
    content: `
      <!-- Replace this if your sign-in process differs. -->
      <p>Enter the email address you registered with and your password. You stay signed in until your session expires, at which point StarterApp returns you to the sign-in screen automatically rather than leaving you on a page whose requests silently fail.</p>
      <h3>If you cannot sign in</h3>
      <ul>
        <li>Check that the email address matches the one you registered with.</li>
        <li>Use <strong>Forgot password</strong> to set a new password.</li>
        <li>Repeated failed attempts are rate limited — wait a moment before trying again.</li>
      </ul>
    `,
    //#else
    description: "Sign in with your organisation account.",
    content: `
      <!-- Replace this if your sign-in process differs. -->
      <p>StarterApp uses your organisation account, so there is no separate password to remember. Choose <strong>Sign in</strong> and complete sign-in with your usual work credentials, including multi-factor authentication if your organisation requires it.</p>
      <h3>If you cannot sign in</h3>
      <ul>
        <li>Confirm you are using your work account rather than a personal one.</li>
        <li>Your account may not have been granted access yet — see Contact Support.</li>
        <li>Password resets are handled by your organisation's IT help desk, not here.</li>
      </ul>
    `,
    //#endif
  },

  //#endif
  "/help/attachments": {
    title: "Attachments",
    description: "Upload files and manage the ones already stored.",
    content: `
      <!-- Replace this with the rules that apply to your files. -->
      <h3>Uploading a file</h3>
      <p>Open the <strong>Attachments</strong> page, enter a reference, choose a file, and select <strong>Upload</strong>. The reference is the key that groups a file with the record it belongs to, so use the same one for every file about the same thing.</p>
      <h3>Size and file types</h3>
      <p>Uploads are capped at 50&nbsp;MB per file by default. If a file is rejected, check its size first. Large files take longer to send on a slow connection — leave the page open until the upload finishes.</p>
      <h3>Removing a file</h3>
      <p>Every uploaded file is listed with its reference, size, and upload date. Selecting <strong>Delete</strong> removes both the stored file and its record. This cannot be undone.</p>
    `,
  },

  //#if (useAuth)
  "/help/admin": {
    title: "Administration",
    description: "For accounts holding the Admin role.",
    content: `
      <!-- Replace this with your own administrative procedures. -->
      <p>The <strong>Admin</strong> item appears in the navigation only for accounts holding the Admin role. It is also enforced on the server, so the pages below cannot be reached by anyone else regardless of how they navigate.</p>
      <h3>Managing users</h3>
      <p>The users panel lists every account and the roles assigned to it. Granting a role takes effect the next time that person signs in.</p>
      <h3>A note on roles</h3>
      <p>Roles decide what a person can do, so treat Admin as the exception rather than the default. Grant it to the smallest group that can still keep the application running.</p>
    `,
  },

  //#endif
  "/help/faq": {
    title: "Frequently Asked Questions",
    description: "Short answers to the questions we are asked most.",
    content: `
      <!-- Replace these with the questions your users actually ask. -->
      <h3>Which browsers are supported?</h3>
      <p>Any current version of Chrome, Edge, Firefox, or Safari. Older browsers may load the page but are not tested against.</p>
      <h3>Does StarterApp work on a phone?</h3>
      <p>Yes. The layout adapts to small screens, and the navigation collapses into a menu.</p>
      <h3>Why was I signed out?</h3>
      <p>Sessions expire after a period of inactivity. Sign in again and you will be returned to the application.</p>
      <h3>My question isn't answered here.</h3>
      <p>See <strong>Contact Support</strong> for how to reach a person.</p>
    `,
  },

  "/help/contact": {
    title: "Contact Support",
    description: "How to reach someone who can help.",
    content: `
      <!-- Replace this with your real support contact details. -->
      <h3>Before you get in touch</h3>
      <p>Check the FAQ first — it covers the most common problems. If you are reporting something that went wrong, note what you were doing, what you expected, and what happened instead. A screenshot usually saves a round trip.</p>
      <h3>Support details</h3>
      <ul>
        <li><strong>Email:</strong> support@example.com</li>
        <li><strong>Phone:</strong> (000)&nbsp;000-0000</li>
        <li><strong>Hours:</strong> Monday to Friday, 9:00&nbsp;a.m. to 5:00&nbsp;p.m.</li>
      </ul>
      <p>Please do not include passwords in anything you send us. Nobody supporting this application will ever ask you for one.</p>
    `,
  },

  "/help/policies": {
    title: "Policies",
    description: "The terms you use StarterApp under.",
    content: `
      <!-- Replace this with a summary of your own policies. -->
      <p>Using StarterApp means accepting the policies below. Each is linked from the footer of every page as well as from the menu on the left.</p>
      <ul>
        <li><strong>Terms of Use</strong> — what you may and may not do here.</li>
        <li><strong>Accessibility</strong> — our commitment, and how to report a barrier.</li>
        <li><strong>Privacy Policy</strong> — what we collect and what we do with it.</li>
      </ul>
    `,
  },

  "/help/terms-of-use": {
    title: "Terms of Use",
    description: "The conditions that apply to your use of StarterApp.",
    content: `
      <!-- Replace this placeholder with your organisation's approved terms.
           Have them reviewed before you publish. -->
      <p>This is placeholder text. Replace it with the terms of use approved by your organisation before this application goes live.</p>
      <h3>Acceptable use</h3>
      <p>Use StarterApp only for its intended purpose, and only with an account issued to you. Do not share your credentials, attempt to reach data belonging to others, or interfere with the operation of the service.</p>
      <h3>Accuracy of information</h3>
      <p>You are responsible for the accuracy of what you submit. We may correct or remove information that is inaccurate.</p>
      <h3>Changes to these terms</h3>
      <p>These terms may change. Continuing to use StarterApp after a change means you accept the revised terms.</p>
    `,
  },

  "/help/accessibility": {
    title: "Accessibility",
    description: "Our commitment, and how to tell us about a barrier.",
    content: `
      <!-- Replace this placeholder with your organisation's accessibility statement. -->
      <p>This is placeholder text. Replace it with your organisation's accessibility statement.</p>
      <h3>Our commitment</h3>
      <p>StarterApp is built to be usable by everyone, including people using screen readers, keyboard navigation, or magnification. We aim to meet WCAG&nbsp;2.1 Level&nbsp;AA.</p>
      <h3>Reporting a barrier</h3>
      <p>If any part of StarterApp is difficult or impossible for you to use, tell us. Include the page you were on and what you were trying to do, and we will treat it as a defect. See <strong>Contact Support</strong> for how to reach us.</p>
    `,
  },

  "/help/privacy": {
    title: "Privacy Policy",
    description: "What we collect, and what we do with it.",
    content: `
      <!-- Replace this placeholder with your organisation's approved privacy policy.
           Have it reviewed before you publish. -->
      <p>This is placeholder text. Replace it with the privacy policy approved by your organisation before this application goes live.</p>
      <h3>What we collect</h3>
      <p>The information you enter, the files you upload, and a record of the actions you take in the application.</p>
      <h3>How we use it</h3>
      <p>To operate the service and to keep it secure. We do not sell it, and we do not use it for advertising.</p>
      <h3>Retention</h3>
      <p>Information is kept for as long as it is needed, and for as long as any applicable record-retention requirement demands.</p>
      <h3>Your choices</h3>
      <p>To ask what is held about you, or to have something corrected, see <strong>Contact Support</strong>.</p>
    `,
  },
};
