/**
 * Canonical Egypt locations for filters, forms, and service-area matching.
 * Governorate maps to AddressDto.state and ServiceAreaDTO.region.
 */
export interface EgyptLocation {
  id: string;
  city: string;
  governorate: string;
  latitude: number;
  longitude: number;
  aliases?: string[];
}

/** All 27 Egyptian governorates (canonical English names). */
export const EGYPT_GOVERNORATES: readonly string[] = [
  'Alexandria',
  'Aswan',
  'Asyut',
  'Beheira',
  'Beni Suef',
  'Cairo',
  'Dakahlia',
  'Damietta',
  'Fayoum',
  'Gharbia',
  'Giza',
  'Ismailia',
  'Kafr El Sheikh',
  'Luxor',
  'Matrouh',
  'Minya',
  'Monufia',
  'New Valley',
  'North Sinai',
  'Port Said',
  'Qalyubia',
  'Qena',
  'Red Sea',
  'Sharqia',
  'Sohag',
  'South Sinai',
  'Suez'
] as const;

export const EGYPT_LOCATIONS: EgyptLocation[] = [
  // Cairo
  {
    id: 'cairo',
    city: 'Cairo',
    governorate: 'Cairo',
    latitude: 30.0444,
    longitude: 31.2357,
    aliases: ['nasr city', 'heliopolis', 'maadi', 'zamalek', 'downtown cairo']
  },
  {
    id: 'new-cairo',
    city: 'New Cairo',
    governorate: 'Cairo',
    latitude: 30.03,
    longitude: 31.47,
    aliases: ['tagamoa', 'fifth settlement', '5th settlement', 'new cairo city']
  },
  // Alexandria
  {
    id: 'alexandria',
    city: 'Alexandria',
    governorate: 'Alexandria',
    latitude: 31.2001,
    longitude: 29.9187,
    aliases: ['alex', 'borg el arab', 'borg el arab city']
  },
  // Giza
  {
    id: 'giza',
    city: 'Giza',
    governorate: 'Giza',
    latitude: 30.0131,
    longitude: 31.2089,
    aliases: ['6th of october', '6 october', 'sheikh zayed', 'dokki', 'haram', 'october city']
  },
  // Qalyubia
  {
    id: 'banha',
    city: 'Banha',
    governorate: 'Qalyubia',
    latitude: 30.4591,
    longitude: 31.1786,
    aliases: ['qalyubia', 'qalyub', 'shubra el kheima', 'el qanater']
  },
  // Port Said
  {
    id: 'port-said',
    city: 'Port Said',
    governorate: 'Port Said',
    latitude: 31.2653,
    longitude: 32.3019,
    aliases: ['portsaid', 'port fouad']
  },
  // Suez
  {
    id: 'suez',
    city: 'Suez',
    governorate: 'Suez',
    latitude: 29.9668,
    longitude: 32.5498,
    aliases: ['suez canal', 'el suez']
  },
  // Damietta
  {
    id: 'damietta',
    city: 'Damietta',
    governorate: 'Damietta',
    latitude: 31.4175,
    longitude: 31.8144,
    aliases: ['dumyat', 'new damietta']
  },
  // Dakahlia
  {
    id: 'mansoura',
    city: 'Mansoura',
    governorate: 'Dakahlia',
    latitude: 31.0409,
    longitude: 31.3785,
    aliases: ['el mansoura', 'mansura', 'dakahlia', 'mit ghamr']
  },
  // Sharqia
  {
    id: 'zagazig',
    city: 'Zagazig',
    governorate: 'Sharqia',
    latitude: 30.5877,
    longitude: 31.5019,
    aliases: ['sharqia', 'sharqiya', '10th of ramadan', '10th ramadan']
  },
  // Kafr El Sheikh
  {
    id: 'kafr-el-sheikh',
    city: 'Kafr El Sheikh',
    governorate: 'Kafr El Sheikh',
    latitude: 31.1107,
    longitude: 30.9388,
    aliases: ['kafr el sheikh city', 'desouk']
  },
  // Gharbia
  {
    id: 'tanta',
    city: 'Tanta',
    governorate: 'Gharbia',
    latitude: 30.7865,
    longitude: 31.0004,
    aliases: ['gharbia', 'gharbiya', 'mahalla', 'el mahalla']
  },
  // Monufia
  {
    id: 'shibin-el-kom',
    city: 'Shibin El Kom',
    governorate: 'Monufia',
    latitude: 30.5549,
    longitude: 31.0123,
    aliases: ['monufia', 'menoufia', 'menouf', 'sadat city']
  },
  // Beheira
  {
    id: 'damanhour',
    city: 'Damanhour',
    governorate: 'Beheira',
    latitude: 31.0341,
    longitude: 30.4682,
    aliases: ['beheira', 'behaira', 'beheira governorate', 'edku']
  },
  // Ismailia
  {
    id: 'ismailia',
    city: 'Ismailia',
    governorate: 'Ismailia',
    latitude: 30.5965,
    longitude: 32.2715,
    aliases: ['el ismailia', 'ismailiya']
  },
  // Beni Suef
  {
    id: 'beni-suef',
    city: 'Beni Suef',
    governorate: 'Beni Suef',
    latitude: 29.0661,
    longitude: 31.0994,
    aliases: ['beni suef city', 'bani suef', 'beni suef governorate']
  },
  // Fayoum
  {
    id: 'fayoum',
    city: 'Fayoum',
    governorate: 'Fayoum',
    latitude: 29.3084,
    longitude: 30.8428,
    aliases: ['faiyum', 'fayum', 'el fayoum']
  },
  // Minya
  {
    id: 'minya',
    city: 'Minya',
    governorate: 'Minya',
    latitude: 28.1099,
    longitude: 30.7503,
    aliases: ['menya', 'el minya', 'minia']
  },
  // Asyut
  {
    id: 'asyut',
    city: 'Asyut',
    governorate: 'Asyut',
    latitude: 27.1783,
    longitude: 31.1859,
    aliases: ['assiut', 'assuit', 'asyut governorate']
  },
  // Sohag
  {
    id: 'sohag',
    city: 'Sohag',
    governorate: 'Sohag',
    latitude: 26.5569,
    longitude: 31.6948,
    aliases: ['sohag city', 'suhag', 'akhmim']
  },
  // Qena
  {
    id: 'qena',
    city: 'Qena',
    governorate: 'Qena',
    latitude: 26.1642,
    longitude: 32.7267,
    aliases: ['qena city', 'nagada', 'naqada']
  },
  // Luxor
  {
    id: 'luxor',
    city: 'Luxor',
    governorate: 'Luxor',
    latitude: 25.6872,
    longitude: 32.6396,
    aliases: ['thebes', 'el luxor']
  },
  // Aswan
  {
    id: 'aswan',
    city: 'Aswan',
    governorate: 'Aswan',
    latitude: 24.0889,
    longitude: 32.8998,
    aliases: ['assuan', 'aswan city']
  },
  // Red Sea
  {
    id: 'hurghada',
    city: 'Hurghada',
    governorate: 'Red Sea',
    latitude: 27.2579,
    longitude: 33.8116,
    aliases: ['red sea', 'el gouna', 'gouna', 'safaga', 'marsa alam']
  },
  // New Valley
  {
    id: 'kharga',
    city: 'Kharga',
    governorate: 'New Valley',
    latitude: 25.4447,
    longitude: 30.5516,
    aliases: ['new valley', 'el kharga', 'wadi el gedid', 'dakhla', 'farafra']
  },
  // Matrouh
  {
    id: 'marsa-matrouh',
    city: 'Marsa Matrouh',
    governorate: 'Matrouh',
    latitude: 31.3543,
    longitude: 27.2373,
    aliases: ['matrouh', 'matruh', 'el alamein', 'alamein']
  },
  {
    id: 'north-coast',
    city: 'North Coast',
    governorate: 'Matrouh',
    latitude: 31.1,
    longitude: 28.5,
    aliases: ['sahel', 'marina', 'hacienda', 'north coast egypt', 'sidi abdel rahman']
  },
  // North Sinai
  {
    id: 'arish',
    city: 'Arish',
    governorate: 'North Sinai',
    latitude: 31.1322,
    longitude: 33.7984,
    aliases: ['el arish', 'north sinai', 'al arish']
  },
  // South Sinai
  {
    id: 'sharm-el-sheikh',
    city: 'Sharm El Sheikh',
    governorate: 'South Sinai',
    latitude: 27.9158,
    longitude: 34.33,
    aliases: ['sharm', 'south sinai', 'dahab', 'nuweiba', 'taba']
  }
];

/** City names for filter chips and dropdowns (canonical display order). */
export const EGYPT_CITY_OPTIONS = EGYPT_LOCATIONS.map((loc) => loc.city);

/** All 27 governorates for governorate pickers. */
export const EGYPT_GOVERNORATE_OPTIONS: string[] = [...EGYPT_GOVERNORATES];

export function getLocationById(id: string): EgyptLocation | undefined {
  return EGYPT_LOCATIONS.find((loc) => loc.id === id);
}

export function getLocationByGovernorate(governorate: string): EgyptLocation | undefined {
  const needle = governorate.trim().toLowerCase();
  if (!needle) return undefined;
  return EGYPT_LOCATIONS.find((loc) => loc.governorate.toLowerCase() === needle);
}

export function getLocationByCity(city: string): EgyptLocation | undefined {
  const needle = city.trim().toLowerCase();
  if (!needle) return undefined;

  const byGovernorate = getLocationByGovernorate(city);
  if (byGovernorate) return byGovernorate;

  return EGYPT_LOCATIONS.find(
    (loc) =>
      loc.city.toLowerCase() === needle ||
      loc.id === needle ||
      loc.aliases?.some((a) => a === needle || needle.includes(a) || a.includes(needle))
  );
}
