import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { QuizAttemptCreateDTO, QuizAttemptResponseDTO } from '../models/models';

@Injectable({ providedIn: 'root' })
export class QuizAttemptService {
  private api = `${environment.apiUrl}/quizattempt`;

  constructor(private http: HttpClient) {}

  submit(dto: QuizAttemptCreateDTO) {
    return this.http.post<QuizAttemptResponseDTO>(this.api, dto);
  }

  getByStudent(studentId: number) {
    return this.http.get<QuizAttemptResponseDTO[]>(`${this.api}/student/${studentId}`);
  }

  getByQuiz(quizId: number) {
    return this.http.get<QuizAttemptResponseDTO[]>(`${this.api}/quiz/${quizId}`);
  }

  getById(id: number) {
    return this.http.get<QuizAttemptResponseDTO>(`${this.api}/${id}`);
  }
}