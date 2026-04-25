/** @type {import('tailwindcss').Config} */
module.exports = {
   darkMode: 'class', 
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        navy: "#1a2540",
        navy2: "#243050",
        gold: "#c9a84c",
        gold2: "#e8c97a",
        cream: "#f9f6f0",
        cream2: "#f0ebe0",
        dark: "#0e1627",
        lgray: "#e8e4dc",
      },
      fontFamily: {
        cormorant: ["'Cormorant Garamond'", "Georgia", "serif"],
        outfit: ["'Outfit'", "sans-serif"],
      }
    },
  },
  plugins: [],
}
