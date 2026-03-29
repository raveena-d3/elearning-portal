import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { DiscussionCreateDTO, DiscussionResponseDTO } from '../models/models';

@Injectable({ providedIn: 'root' })
export class DiscussionService {
  private api = `${environment.apiUrl}/discussion`;
  constructor(private http: HttpClient) {}

  askQuestion(dto: DiscussionCreateDTO) { return this.http.post<DiscussionResponseDTO>(this.api, dto); }
  answerQuestion(id: number, answer: string) { return this.http.put<DiscussionResponseDTO>(`${this.api}/${id}/answer`, { answer }); }
  getByCourse(courseId: number) { return this.http.get<DiscussionResponseDTO[]>(`${this.api}/course/${courseId}`); }
  getByStudent(studentId: number) { return this.http.get<DiscussionResponseDTO[]>(`${this.api}/student/${studentId}`); }
  delete(id: number) { return this.http.delete(`${this.api}/${id}`); }
  getMyCoursesDiscussions() {
    return this.http.get<DiscussionResponseDTO[]>(`${this.api}/my-courses`);
}
}   