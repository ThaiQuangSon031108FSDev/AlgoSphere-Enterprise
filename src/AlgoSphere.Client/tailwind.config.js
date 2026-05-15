/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        'brand-slate': '#0F172A',
        'brand-emerald': '#10B981',
        'brand-indigo': '#6366F1',
      }
    },
  },
  plugins: [],
}
