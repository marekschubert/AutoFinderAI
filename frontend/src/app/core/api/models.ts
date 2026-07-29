// Typed DTOs mirroring backend API contracts (enums serialize as string names).

export type FuelType = 'Unknown' | 'Petrol' | 'Diesel' | 'Lpg' | 'Hybrid' | 'Electric' | 'Hydrogen';
export type TransmissionType = 'Unknown' | 'Manual' | 'Automatic';
export type BodyType =
  | 'Unknown'
  | 'Sedan'
  | 'Hatchback'
  | 'Kombi'
  | 'Suv'
  | 'Coupe'
  | 'Convertible'
  | 'Van'
  | 'Pickup';
export type DriveType = 'Unknown' | 'FrontWheel' | 'RearWheel' | 'AllWheel';
export type MessageRole = 'User' | 'Assistant';
export type CrawlStatus = 'Running' | 'Completed' | 'Failed';
export type VehicleSortBy = 'Relevance' | 'PriceAsc' | 'PriceDesc' | 'YearDesc' | 'MileageAsc';

// --- Auth ---

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResult {
  userId: string;
  email: string;
  token: string;
}

export interface MeResult {
  userId: string;
  email: string;
  createdAt: string;
}

// --- Chat ---

export interface ChatSessionSummaryDto {
  id: string;
  title: string;
  createdAt: string;
  lastMessageAt: string;
}

export interface ChatMessageDto {
  id: string;
  role: MessageRole;
  content: string;
  criteriaJson: string | null;
  resultVehicleIdsJson: string | null;
  modelUsed: string | null;
  createdAt: string;
  results?: VehicleDto[] | null;
}

export interface ChatSessionDetailDto {
  id: string;
  title: string;
  createdAt: string;
  lastMessageAt: string;
  messages: ChatMessageDto[];
}

export interface CreateSessionResult {
  id: string;
  title: string;
  createdAt: string;
  lastMessageAt: string;
}

export interface SendMessageRequest {
  content: string;
}

export interface VehicleSearchCriteria {
  make?: string | null;
  model?: string | null;
  minPrice?: number | null;
  maxPrice?: number | null;
  minYear?: number | null;
  maxYear?: number | null;
  maxMileage?: number | null;
  fuelType?: FuelType | null;
  transmission?: TransmissionType | null;
  bodyType?: BodyType | null;
  minPowerHp?: number | null;
  keywords?: string[] | null;
  maxPowerHp?: number | null;
  seatsMin?: number | null;
  excludeDamaged?: boolean | null;
  locationContains?: string | null;
  sortBy: VehicleSortBy;
  limit?: number | null;
  softPreferences?: string[] | null;
}

export interface VehicleDto {
  id: string;
  url: string;
  title: string;
  priceAmount: number;
  priceCurrency: string;
  make: string;
  model: string;
  version: string | null;
  productionYear: number;
  mileage: number | null;
  fuelType: FuelType;
  transmission: TransmissionType;
  enginePowerHp: number | null;
  engineCapacityCm3: number | null;
  location: string | null;
  thumbnailUrl: string | null;
  publishedAt: string;
  bodyType: BodyType;
  doors: number | null;
  seats: number | null;
  driveType: DriveType | null;
  color: string | null;
  isDamaged: boolean | null;
  isFirstOwner: boolean | null;
  countryOfOrigin: string | null;
}

export interface SendMessageResult {
  assistantMessage: ChatMessageDto;
  criteria: VehicleSearchCriteria | null;
  results: VehicleDto[];
  clarificationQuestion: string | null;
}

// --- AI ---

export interface AiStatus {
  available: boolean;
  defaultModel: string | null;
  allowKeywordFallback: boolean;
}

export interface AiModels {
  defaultModel: string | null;
  models: string[];
}

// --- Crawl ---

export interface CrawlRunDto {
  id: string;
  sourceKey: string;
  category: string;
  startedAt: string;
  finishedAt: string | null;
  status: CrawlStatus;
  itemsFound: number;
  itemsSaved: number;
  error: string | null;
}
