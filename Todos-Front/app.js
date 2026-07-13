const todoForm = document.querySelector('#todo-form');
const todoList = document.querySelector('#todo-list');
const statusElement = document.querySelector('#status');
const refreshButton = document.querySelector('#refresh-button');
const todoTemplate = document.querySelector('#todo-template');

const configuredApiLocalUrl = window.TODOS_CONFIG?.apiLocalUrl;
const configuredApiProdUrl = window.TODOS_CONFIG?.apiProdUrl;
const defaultApiBaseUrl = window.location.hostname === 'localhost' ? configuredApiLocalUrl : configuredApiProdUrl;
const apiBaseUrl = normalizeBaseUrl(defaultApiBaseUrl);

todoForm.addEventListener('submit', async (event) => {
  event.preventDefault();

  const formData = new FormData(todoForm);
  const description = normalizeDescription(formData.get('description'));
  const done = formData.get('done') === 'on';

  await runAction(async () => {
    await request('/api/Todos', {
      method: 'POST',
      body: JSON.stringify({ description, done }),
    });

    todoForm.reset();
    await loadTodos();
  }, 'Nie udało się dodać zadania.');
});

refreshButton.addEventListener('click', loadTodos);

loadTodos();

async function loadTodos() {
  await runAction(async () => {
    setStatus('Ładowanie...');
    const todos = await request('/api/Todos');
    renderTodos(todos);
    setStatus(todos.length ? `Zadań: ${todos.length}` : 'Brak zadań.');
  }, 'Nie udało się pobrać listy zadań.');
}

function renderTodos(todos) {
  todoList.replaceChildren();

  for (const todo of todos) {
    const item = todoTemplate.content.firstElementChild.cloneNode(true);
    const form = item.querySelector('.edit-form');
    const doneInput = item.querySelector('.todo-done');
    const descriptionInput = item.querySelector('.todo-description');
    const createdElement = item.querySelector('.todo-created');
    const deleteButton = item.querySelector('.delete-button');

    doneInput.checked = Boolean(todo.done);
    descriptionInput.value = todo.description || '';
    createdElement.dateTime = todo.createdAt || '';
    createdElement.textContent = formatDate(todo.createdAt);
    item.classList.toggle('is-done', Boolean(todo.done));

    form.addEventListener('submit', async (event) => {
      event.preventDefault();

      await runAction(async () => {
        const updated = await request(`/api/Todos/${todo.id}`, {
          method: 'PUT',
          body: JSON.stringify({
            description: normalizeDescription(descriptionInput.value),
            done: doneInput.checked,
          }),
        });

        item.classList.toggle('is-done', Boolean(updated.done));
        setStatus('Zapisano zmiany.');
        await loadTodos();
      }, 'Nie udało się zapisać zadania.');
    });

    doneInput.addEventListener('change', () => {
      form.requestSubmit();
    });

    deleteButton.addEventListener('click', async () => {
      const confirmed = window.confirm('Usunąć to zadanie?');

      if (!confirmed) {
        return;
      }

      await runAction(async () => {
        await request(`/api/Todos/${todo.id}`, { method: 'DELETE' });
        item.remove();
        await loadTodos();
      }, 'Nie udało się usunąć zadania.');
    });

    todoList.append(item);
  }
}

async function request(path, options = {}) {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
      ...options.headers,
    },
    ...options,
  });

  if (!response.ok) {
    const problem = await readProblem(response);
    throw new Error(problem || `HTTP ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

async function readProblem(response) {
  try {
    const data = await response.json();
    return data.detail || data.title || null;
  } catch {
    return null;
  }
}

async function runAction(action, fallbackMessage) {
  try {
    await action();
  } catch (error) {
    console.error(error);
    setStatus(`${fallbackMessage} ${error.message}`, true);
  }
}

function setStatus(message, isError = false) {
  statusElement.textContent = message;
  statusElement.classList.toggle('is-error', isError);
}

function normalizeBaseUrl(value) {
  return String(value || defaultApiBaseUrl).trim().replace(/\/$/, '');
}

function normalizeDescription(value) {
  const description = String(value || '').trim();
  return description || null;
}

function formatDate(value) {
  if (!value) {
    return '';
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat('pl-PL', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(date);
}