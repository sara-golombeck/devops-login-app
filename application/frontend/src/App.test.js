import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import App from './App';

// Mock fetch
global.fetch = jest.fn();

describe('App Component', () => {
  beforeEach(() => {
    fetch.mockClear();
  });

  test('renders email login form', () => {
    render(<App />);
    
    expect(screen.getByText('Email Login')).toBeInTheDocument();
    expect(screen.getByLabelText(/email address/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /login/i })).toBeInTheDocument();
  });

  test('shows error for empty email', async () => {
    render(<App />);
    
    const loginButton = screen.getByRole('button', { name: /login/i });
    fireEvent.click(loginButton);
    
    expect(screen.getByText('Please enter an email address')).toBeInTheDocument();
  });

  test('shows loading state when submitting', async () => {
    fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ success: true, message: 'Email sent' })
    });

    render(<App />);
    
    const emailInput = screen.getByLabelText(/email address/i);
    const loginButton = screen.getByRole('button', { name: /login/i });
    
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.click(loginButton);
    
    expect(screen.getByText('Sending...')).toBeInTheDocument();
    expect(loginButton).toBeDisabled();
  });

  test('shows success message on successful login', async () => {
    fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ 
        success: true, 
        message: 'Login email sent successfully!' 
      })
    });

    render(<App />);
    
    const emailInput = screen.getByLabelText(/email address/i);
    const loginButton = screen.getByRole('button', { name: /login/i });
    
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.click(loginButton);
    
    await waitFor(() => {
      expect(screen.getByText(/login email sent successfully/i)).toBeInTheDocument();
    });
    
    expect(emailInput).toHaveValue('');
  });

  test('shows error message on failed login', async () => {
    fetch.mockResolvedValueOnce({
      ok: false,
      json: async () => ({ 
        success: false, 
        message: 'Invalid email format' 
      })
    });

    render(<App />);
    
    const emailInput = screen.getByLabelText(/email address/i);
    const loginButton = screen.getByRole('button', { name: /login/i });
    
    fireEvent.change(emailInput, { target: { value: 'invalid-email' } });
    fireEvent.click(loginButton);
    
    await waitFor(() => {
      expect(screen.getByText('Invalid email format')).toBeInTheDocument();
    });
  });

  test('makes correct API call', async () => {
    fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ success: true })
    });

    render(<App />);
    
    const emailInput = screen.getByLabelText(/email address/i);
    const loginButton = screen.getByRole('button', { name: /login/i });
    
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.click(loginButton);
    
    expect(fetch).toHaveBeenCalledWith('/api/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email: 'test@example.com' }),
    });
  });
});