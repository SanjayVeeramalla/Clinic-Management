// Vite requires explicit .jsx extensions in imports (unlike CRA which resolved them automatically)

import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { AuthProvider } from './context/AuthContext.jsx';
import ProtectedRoute from './components/Shared/ProtectedRoute.jsx';
import Navbar from './components/Shared/Navbar.jsx';

// Auth
import Login    from './components/Auth/Login.jsx';
import Register from './components/Auth/Register.jsx';

// Patient
import PatientDashboard from './components/Patient/PatientDashboard.jsx';
import BookAppointment  from './components/Patient/BookAppointment.jsx';
import MyAppointments   from './components/Patient/MyAppointments.jsx';
import PatientProfile   from './components/Patient/PatientProfile.jsx';

// Doctor
import DoctorDashboard    from './components/Doctor/DoctorDashboard.jsx';
import DoctorAppointments from './components/Doctor/DoctorAppointments.jsx';
import DoctorSchedule     from './components/Doctor/DoctorSchedule.jsx';
import PrescriptionForm   from './components/Doctor/PrescriptionForm.jsx';

// Admin
import AdminDashboard from './components/Admin/AdminDashboard.jsx';
import ManageDoctors  from './components/Admin/ManageDoctors.jsx';
import ManagePatients from './components/Admin/ManagePatients.jsx';
import Reports        from './components/Admin/Reports.jsx';

import './App.css';

const AppLayout = ({ children }) => (
  <>
    <Navbar />
    <main className="main-content">{children}</main>
  </>
);

function App() {
  return (
    <AuthProvider>
      <Router>
        <Toaster position="top-right" toastOptions={{ duration: 3500 }} />
        <Routes>
          {/* Public */}
          <Route path="/"         element={<Navigate to="/login" replace />} />
          <Route path="/login"    element={<Login />} />
          <Route path="/register" element={<Register />} />

          {/* Patient */}
          <Route path="/patient/dashboard" element={
            <ProtectedRoute roles={['Patient']}>
              <AppLayout><PatientDashboard /></AppLayout>
            </ProtectedRoute>
          } />
          <Route path="/patient/book-appointment" element={
            <ProtectedRoute roles={['Patient']}>
              <AppLayout><BookAppointment /></AppLayout>
            </ProtectedRoute>
          } />
          <Route path="/patient/appointments" element={
            <ProtectedRoute roles={['Patient']}>
              <AppLayout><MyAppointments /></AppLayout>
            </ProtectedRoute>
          } />
          <Route path="/patient/profile" element={
            <ProtectedRoute roles={['Patient']}>
              <AppLayout><PatientProfile /></AppLayout>
            </ProtectedRoute>
          } />

          {/* Doctor */}
          <Route path="/doctor/dashboard" element={
            <ProtectedRoute roles={['Doctor']}>
              <AppLayout><DoctorDashboard /></AppLayout>
            </ProtectedRoute>
          } />
          <Route path="/doctor/appointments" element={
            <ProtectedRoute roles={['Doctor']}>
              <AppLayout><DoctorAppointments /></AppLayout>
            </ProtectedRoute>
          } />
          <Route path="/doctor/schedule" element={
            <ProtectedRoute roles={['Doctor']}>
              <AppLayout><DoctorSchedule /></AppLayout>
            </ProtectedRoute>
          } />
          <Route path="/doctor/prescription/:appointmentId" element={
            <ProtectedRoute roles={['Doctor']}>
              <AppLayout><PrescriptionForm /></AppLayout>
            </ProtectedRoute>
          } />

          {/* Admin */}
          <Route path="/admin/dashboard" element={
            <ProtectedRoute roles={['Admin']}>
              <AppLayout><AdminDashboard /></AppLayout>
            </ProtectedRoute>
          } />
          <Route path="/admin/doctors" element={
            <ProtectedRoute roles={['Admin']}>
              <AppLayout><ManageDoctors /></AppLayout>
            </ProtectedRoute>
          } />
          <Route path="/admin/patients" element={
            <ProtectedRoute roles={['Admin']}>
              <AppLayout><ManagePatients /></AppLayout>
            </ProtectedRoute>
          } />
          <Route path="/admin/reports" element={
            <ProtectedRoute roles={['Admin']}>
              <AppLayout><Reports /></AppLayout>
            </ProtectedRoute>
          } />

          {/* Error pages */}
          <Route path="/unauthorized" element={
            <div className="error-page">
              <h2>403 — Unauthorized</h2>
              <p>You don't have permission to access this page.</p>
              <a href="/login" className="btn btn-primary">Go to Login</a>
            </div>
          } />
          <Route path="*" element={
            <div className="error-page">
              <h2>404 — Page Not Found</h2>
              <a href="/" className="btn btn-primary">Go Home</a>
            </div>
          } />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;