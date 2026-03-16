export type UserRole = 'Admin' | 'Vendor' | 'User' | null;

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
    value: {
        user: UserSession;
        token: string;
        refreshToken: string;
        role: UserRole;
    }
}

export interface APIResponse<T> {
    value: T;
}



