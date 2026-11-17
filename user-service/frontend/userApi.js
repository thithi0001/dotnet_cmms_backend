import axios from 'axios'

const userApi = axios.create({
  baseURL: import.meta.env.VITE_USER_API_BASE_URL ?? 'http://localhost:5087/api',
  headers: { Accept: 'application/json' },
})

export const getUsers = () => userApi.get('/users').then(({ data }) => data)
export const getUser = (id) => userApi.get(`/users/${id}`).then(({ data }) => data)
export const createUser = (user) => userApi.post('/users', user).then(({ data }) => data)
export const updateUser = (id, user) =>
  userApi.put(`/users/${id}`, user).then(({ data }) => data)
export const deleteUser = (id) => userApi.delete(`/users/${id}`)
