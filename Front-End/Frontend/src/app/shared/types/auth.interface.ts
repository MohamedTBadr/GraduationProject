export type UserRole = 'admin' | 'vendor' | 'user' | null;

export interface UserSession {
    id: string;
    name: string;
    role: UserRole;
    email: string;
}

export interface LoginCredentials {
    email: string;
    password?: string;
    role?: UserRole; // for mock selection
}

export interface AuthResponse {
    user: UserSession;
    token: string;
}
