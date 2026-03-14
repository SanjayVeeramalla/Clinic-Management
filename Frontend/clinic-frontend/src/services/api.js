import axios from 'axios';

const API_BASE = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

// ─── Axios instance ───────────────────────────────────────────────────────────
const api = axios.create({
  baseURL: API_BASE,
  headers: { 'Content-Type': 'application/json' },
});

// Request interceptor — attach JWT from localStorage
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor — auto-refresh on 401
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      const refreshToken = localStorage.getItem('refreshToken');

      if (refreshToken) {
        try {
          const res = await axios.post(`${API_BASE}/auth/refresh-token`, { refreshToken });
          const { accessToken, refreshToken: newRefresh } = res.data.data;
          localStorage.setItem('accessToken', accessToken);
          localStorage.setItem('refreshToken', newRefresh);
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
          return api(originalRequest);
        } catch {
          localStorage.clear();
          window.location.href = '/login';
        }
      } else {
        localStorage.clear();
        window.location.href = '/login';
      }
    }

    return Promise.reject(error);
  }
);

// ─── Auth endpoints ───────────────────────────────────────────────────────────
export const authAPI = {
  register:     (data)         => api.post('/auth/register', data),
  login:        (data)         => api.post('/auth/login', data),
  logout:       (refreshToken) => api.post('/auth/logout', { refreshToken }),
  refreshToken: (refreshToken) => api.post('/auth/refresh-token', { refreshToken }),
};

// ─── Doctors endpoints ────────────────────────────────────────────────────────
export const doctorAPI = {
  getAll:                 (params)               => api.get('/doctor', { params }),
  getById:                (id)                   => api.get(`/doctor/${id}`),
  getProfile:             ()                     => api.get('/doctor/profile'),
  update:                 (id, data)             => api.put(`/doctor/${id}`, data),
  getAppointments:        (id, params)           => api.get(`/doctor/${id}/appointments`, { params }),
  getAvailableSlots:      (id, date)             => api.get(`/doctor/${id}/available-slots`, { params: { date } }),
  setSchedule:            (id, data)             => api.post(`/doctor/${id}/schedule`, data),
  updateAppointmentStatus:(appointmentId, data)  => api.put(`/doctor/appointments/${appointmentId}/status`, data),
  addPrescription:        (appointmentId, data)  => api.post(`/doctor/appointments/${appointmentId}/prescription`, data),
};

// ─── Patient endpoints ────────────────────────────────────────────────────────
export const patientAPI = {
  getProfile:        ()           => api.get('/patient/profile'),
  updateProfile:     (data)       => api.put('/patient/profile', data),
  bookAppointment:   (data)       => api.post('/patient/appointments', data),
  getAppointments:   (params)     => api.get('/patient/appointments', { params }),
  getAppointment:    (id)         => api.get(`/patient/appointments/${id}`),
  cancelAppointment: (id, data)   => api.delete(`/patient/appointments/${id}`, { data }),
};

// ─── Admin endpoints ──────────────────────────────────────────────────────────
export const adminAPI = {
  getDashboard:            ()       => api.get('/admin/dashboard'),
  getDoctors:              ()       => api.get('/admin/doctors'),
  createDoctorAccount:     (data)   => api.post('/admin/doctors/create-account', data),
  updateDoctor:            (id, d)  => api.put(`/admin/doctors/${id}`, d),
  getPatients:             (search) => api.get('/admin/patients', { params: { search } }),
  getPatient:              (id)     => api.get(`/admin/patients/${id}`),
  deactivateUser:          (id)     => api.put(`/admin/users/${id}/deactivate`),
  appointmentSummaryReport:(data)   => api.post('/admin/reports/appointments', data),
  doctorWorkloadReport:    (data)   => api.post('/admin/reports/doctor-workload', data),
};

// ─── Specializations ──────────────────────────────────────────────────────────
export const specializationsAPI = {
  getAll: () => api.get('/specializations'),
};

export default api;
