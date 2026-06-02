import { Vendor } from '../types/vendor.interface';

const DEFAULT_SERVICES = [
    { icon: '', name: 'Main Package', price: '15,000 EGP', desc: 'Full-service offering for your event — setup, coordination, and teardown included.', delivery: '2-4 weeks planning', duration: 'Full event day' },
    { icon: '', name: 'Standard Service', price: '9,000 EGP', desc: 'Essential coverage covering all the core elements at a great price.', delivery: '1-2 weeks planning', duration: '6 hours' },
    { icon: '', name: 'Premium Add-on', price: 'Price on request', desc: 'Upgraded luxury materials and priority scheduling for exclusive events.', delivery: 'Varies', duration: 'Flexible' }
];

const DEFAULT_PACKAGES = [
    { icon: '', name: 'Full Event Package', price: '22,500 EGP', desc: 'Everything included. Complete management, premium materials, hassle-free delivery.' },
    { icon: '', name: 'Budget Package', price: '10,000 EGP', desc: 'Core services at a great price. Perfect for smaller events or tighter budgets.' }
];

export const MOCK_VENDORS: Vendor[] = [
    {
        id: 1, name: 'White Rose Decor', vendorTypeName: 'Decoration', location: 'New Cairo', rating: 4.9, reviews: 124, startPrice: 5000, emoji: '',
        events: ['Wedding', 'Engagement'],
        services: DEFAULT_SERVICES,
        packages: DEFAULT_PACKAGES,
        about: 'Founded in 2016, White Rose Decor has become one of Cairo\'s most trusted event service providers. We specialize in creating environments that feel both luxurious and personal.',
        status: 'active', joined: 'Jan 2024', bookings: 45, revenue: '120,400',
        amenities: ['Floral Design', 'Custom Table Settings', 'Stage Setup', 'Lighting Control']
    },
    {
        id: 2, name: 'Royal Hall Cairo', vendorTypeName: 'Venue', location: 'Heliopolis', rating: 4.8, reviews: 89, startPrice: 20000, emoji: '',
        events: ['Wedding', 'Corporate', 'Graduation'],
        services: DEFAULT_SERVICES,
        packages: DEFAULT_PACKAGES,
        about: 'A prestigious venue in the heart of Heliopolis offering luxury ballroom settings for weddings and high-end corporate events.',
        status: 'active', joined: 'Feb 2024', bookings: 32, revenue: '640,000'
    },
    {
        id: 3, name: 'Studio Lens', vendorTypeName: 'Photography', location: 'Giza', rating: 4.9, reviews: 310, startPrice: 3000, emoji: '',
        events: ['Wedding', 'Engagement', 'Birthday'],
        services: DEFAULT_SERVICES,
        packages: DEFAULT_PACKAGES,
        about: 'Capturing your most precious moments with cinematic quality. Our team of expert photographers ensures every detail is preserved forever.',
        status: 'active', joined: 'Mar 2024', bookings: 88, revenue: '264,000'
    },
    {
        id: 4, name: 'Elite Catering', vendorTypeName: 'Catering', location: 'Cairo', rating: 4.7, reviews: 92, startPrice: 8000, emoji: '',
        events: ['Wedding', 'Birthday', 'Corporate'],
        services: DEFAULT_SERVICES,
        packages: DEFAULT_PACKAGES,
        status: 'pending', applied: 'Mar 2', docs: 'Under Review'
    },
    {
        id: 5, name: 'DJ Mido Events', vendorTypeName: 'Entertainment', location: 'Alexandria', rating: 4.6, reviews: 205, startPrice: 4000, emoji: '',
        events: ['Wedding', 'Birthday', 'Corporate'],
        services: DEFAULT_SERVICES,
        packages: DEFAULT_PACKAGES,
        status: 'active'
    },
    {
        id: 6, name: 'Sweet Dreams Cakes', vendorTypeName: 'Catering', location: 'New Cairo', rating: 4.9, reviews: 45, startPrice: 1500, emoji: '',
        status: 'active'
    },
    {
        id: 7, name: 'Nile City Venue', vendorTypeName: 'Venue', location: 'Cairo', rating: 4.9, reviews: 412, startPrice: 35000, emoji: '',
        status: 'active'
    },
    {
        id: 8, name: 'Elite Events', vendorTypeName: 'Decoration', location: 'New Cairo', rating: 4.8, reviews: 156, startPrice: 12000, emoji: '',
        status: 'active'
    },
    {
        id: 9, name: 'Lumiere Studios', vendorTypeName: 'Photography', location: 'Maadi', rating: 4.7, reviews: 67, startPrice: 4500, emoji: '',
        status: 'active'
    },
    {
        id: 10, name: 'The Golden Plate', vendorTypeName: 'Catering', location: 'Giza', rating: 4.9, reviews: 189, startPrice: 15000, emoji: '',
        status: 'active'
    }
];

