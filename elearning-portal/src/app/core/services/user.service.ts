import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { UserResponseDTO } from '../models/models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private api = `${environment.apiUrl}/user`;
  constructor(private http: HttpClient) {}

  getAll() { return this.http.get<UserResponseDTO[]>(this.api); }
  getById(id: number) { return this.http.get<UserResponseDTO>(`${this.api}/${id}`); }
  updateRole(id: number, role: string) { return this.http.put<UserResponseDTO>(`${this.api}/${id}/role`, { role }); }
  create(dto: { username: string; password: string; role: string }) {
  return this.http.post<UserResponseDTO>(`${this.api}`, dto);
}
  delete(id: number) { return this.http.delete(`${this.api}/${id}`); }
}
