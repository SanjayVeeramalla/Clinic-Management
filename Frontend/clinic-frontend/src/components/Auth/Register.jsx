import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import toast from 'react-hot-toast';

const Register = () => {
  const { register } = useAuth();
  const navigate = useNavigate();

  const [form, setForm] = useState({
    fullName: '', email: '', password: '', confirmPassword: '', phone: '',
    // Role removed — public registration is Patients only.
    // Doctors are created by Admin from the admin dashboard.
  });
  const [loading, setLoading] = useState(false);
  const [error, setError]     = useState('');

  const handleChange = (e) =>
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (form.password !== form.confirmPassword) {
      setError('Passwords do not match');
      return;
    }
    if (form.password.length < 6) {
      setError('Password must be at least 6 characters');
      return;
    }

    setLoading(true);
    try {
      const { confirmPassword, ...payload } = form;
      // payload has no role field — backend hardcodes Patient
      await register(payload);
      toast.success('Account created successfully!');
      navigate('/patient/dashboard', { replace: true });
    } catch (err) {
      setError(err.response?.data?.message || 'Registration failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <div className="auth-header">
          <h2>🏥 Create Account</h2>
          <p>Register as a patient</p>
        </div>

        {error && <div className="alert alert-error">⚠️ {error}</div>}

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label>Full Name</label>
            <input name="fullName" type="text" value={form.fullName}
              onChange={handleChange} placeholder="John Doe" required />
          </div>

          <div className="form-group">
            <label>Email Address</label>
            <input name="email" type="email" value={form.email}
              onChange={handleChange} placeholder="you@example.com" required />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Password</label>
              <input name="password" type="password" value={form.password}
                onChange={handleChange} placeholder="Min 6 characters" required minLength={6} />
            </div>
            <div className="form-group">
              <label>Confirm Password</label>
              <input name="confirmPassword" type="password" value={form.confirmPassword}
                onChange={handleChange} placeholder="Repeat password" required />
            </div>
          </div>

          <div className="form-group">
            <label>Phone (optional)</label>
            <input name="phone" type="tel" value={form.phone}
              onChange={handleChange} placeholder="9XXXXXXXXX" />
          </div>

          <button type="submit" className="btn btn-primary btn-full btn-lg" disabled={loading}>
            {loading ? 'Creating account...' : 'Create Patient Account'}
          </button>
        </form>

        <p className="auth-footer">
          Already have an account? <Link to="/login">Sign in</Link>
        </p>

        <div style={{
          marginTop: '1.25rem', padding: '0.875rem',
          background: '#eff6ff', borderRadius: '8px',
          fontSize: '0.8rem', color: '#1d4ed8',
          border: '1px solid #bfdbfe'
        }}>
          👨‍⚕️ <strong>Are you a doctor?</strong> Doctor accounts are created by the clinic admin. Please contact your administrator.
        </div>
      </div>
    </div>
  );
};

export default Register;
