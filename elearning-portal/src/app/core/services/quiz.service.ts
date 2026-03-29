import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { QuizCreateDTO, QuizResponseDTO } from '../models/models';

@Injectable({ providedIn: 'root' })
export class QuizService {
  private api = `${environment.apiUrl}/quiz`;
  constructor(private http: HttpClient) {}

  create(dto: QuizCreateDTO) { return this.http.post<QuizResponseDTO>(this.api, dto); }
  getByCourse(courseId: number) { return this.http.get<QuizResponseDTO[]>(`${this.api}/course/${courseId}`); }
  getById(id: number) { return this.http.get<QuizResponseDTO>(`${this.api}/${id}`); }
  delete(id: number) { return this.http.delete(`${this.api}/${id}`); }
  getMyCoursesQuizzes()                   { return this.http.get<QuizResponseDTO[]>(`${this.api}/my-courses`); }

}