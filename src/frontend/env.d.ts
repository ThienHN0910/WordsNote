/// <reference types="vite/client" />

interface ImportMetaEnv {
	readonly VITE_APP_API_URL: string
	readonly VITE_GOOGLE_CLIENT_ID: string
	readonly VITE_GOOGLE_ALLOWED_EMAIL: string
}

interface ImportMeta {
	readonly env: ImportMetaEnv
}

interface GoogleIdentityAccountsId {
	initialize: (options: {
		client_id: string
		callback: (response: { credential?: string }) => void
	}) => void
	renderButton: (element: HTMLElement, options: {
		theme?: 'outline' | 'filled_blue' | 'filled_black'
		size?: 'large' | 'medium' | 'small'
		shape?: 'rectangular' | 'pill' | 'circle' | 'square'
		text?: 'signin_with' | 'signup_with' | 'continue_with' | 'signin'
		width?: number
	}) => void
}

interface Window {
	google?: {
		accounts?: {
			id?: GoogleIdentityAccountsId
		}
	}
}
