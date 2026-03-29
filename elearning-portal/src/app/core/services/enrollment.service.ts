import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EnrollmentResponseDTO } from '../models/models';

export interface EnrollmentCreateDTO {
  studentId: number;
  courseId: number;
}

@Injectable({ providedIn: 'root' })
export class EnrollmentService {
  private api = `${environment.apiUrl}/enrollment`;

  constructor(private http: HttpClient) {}

  getMyEnrollments(): Observable<EnrollmentResponseDTO[]> {
    return this.http.get<EnrollmentResponseDTO[]>(`${this.api}/my`);
  }

  getByStudent(studentId: number): Observable<EnrollmentResponseDTO[]> {
    return this.http.get<EnrollmentResponseDTO[]>(
      `${this.api}/student/${studentId}`
    );
  }

  getByCourse(courseId: number): Observable<EnrollmentResponseDTO[]> {
    return this.http.get<EnrollmentResponseDTO[]>(
      `${this.api}/course/${courseId}`
    );
  }

  enroll(courseId: number): Observable<EnrollmentResponseDTO> {
    return this.http.post<EnrollmentResponseDTO>(this.api, {
      studentId: 0,
      courseId: courseId
    });
  }

  unenroll(id: number): Observable<void> {
    return this.http.delete<void>(`${this.api}/${id}`);
  }
  checkEnrollment(courseId:number):Observable<{isEnrolled:boolean}>{
    return this.http.get<{isEnrolled :boolean}>(`${this.api}/check/${courseId}`);
  }
}