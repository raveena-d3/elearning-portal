
export interface CreateUserDTO {
  username: string;
  password: string;
  role: string;
}

export interface UserResponseDTO {
  id: number;
  username: string;
  role: string;
}

export interface CourseCreateDTO {
  title: string;
  description: string;
  instructorId: number;
  youtubeLinks?:string[];
}

export interface CourseResponseDTO {
  id: number;
  title: string;
  description: string;
  instructorId: number;
  instructorName: string;
  youtubeLinks?:string[];
  videoFileName?:string;
  videoOriginalName?: string;
}

export interface EnrollmentCreateDTO {
  courseId: number;
  studentId: number;
}

export interface EnrollmentResponseDTO {
  id: number;
  courseId: number;
  courseTitle: string;
  studentId: number;
  studentName: string;
  enrolledAt: string;
}

export interface QuizCreateDTO {
  title: string;
  questionsJson: string;
  courseId: number;
  instructorId: number;
}

export interface QuizResponseDTO {
  id: number;
  title: string;
  courseId: number;
  courseTitle: string;
  instructorId: number;
  instructorName: string;
}

export interface DiscussionCreateDTO {
  courseId: number;
  studentId: number;
  question: string;
}

export interface DiscussionResponseDTO {
  id: number;
  courseId: number;
  courseTitle: string;
  studentId: number;
  studentName: string;
  question: string;
  answer: string | null;
  askedAt: string;
  answeredAt: string | null;
}

export interface QuizQuestion {
  question: string;
  options: string[];
  correctAnswer: string;
}
export interface QuizAttemptCreateDTO {
  quizId: number;
  studentId: number;
  answers: string[];
}

export interface QuizAttemptResponseDTO {
  id: number;
  quizId: number;
  quizTitle: string;
  studentId: number;
  studentName: string;
  score: number;
  totalQuestions: number;
  percentage: number;
  answersJson: string;
  attemptedAt: string;
}

export interface QuizQuestion {
  question: string;
  options: string[];
  correctAnswer: string;
}
